import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, debounceTime, Subject, takeUntil, tap } from 'rxjs';
import { RedirectorService, ServerSlot, ServerStatisticsResponse, SnackbarManager, UpdateServerSlotRequest } from '../../../shared';
import { ServerSlotDialogManager, ServerSlotService, ServerStatisticsService } from '../../index';

export enum ServerStatus {
  Online = 'Online',
  Offline = 'Offline',
  NoData = 'No Data'
}

@Component({
  selector: 'server-slot',
  templateUrl: './server-slot.component.html',
  styleUrls: ['./server-slot.component.scss'],
})
export class ServerSlotComponent implements AfterViewInit, OnInit, OnDestroy {
  @Input({ required: true }) serverSlot!: ServerSlot;
  @ViewChild('textSizer', { static: false }) textSizer!: ElementRef;

  inputControl = new FormControl('');
  inputIsEditable$ = new BehaviorSubject<boolean>(false);
  inputWidth$ = new BehaviorSubject<number>(120);
  serverStatus$ = new BehaviorSubject<ServerStatus>(ServerStatus.NoData);
  private destroy$ = new Subject<void>();

  constructor(
    private readonly serverSlotService: ServerSlotService,
    private readonly dialogManager: ServerSlotDialogManager,
    private readonly redirector: RedirectorService,
    private readonly snackBarManager: SnackbarManager,
    private readonly serverStatisticsService: ServerStatisticsService,
  ) { }

  ngOnInit(): void {
    this.initializeServerSlot();
    this.initializeStatisticsSubscription();

    this.inputControl.valueChanges.pipe(
      debounceTime(300),
      tap(() => this.adjustInputWidth()),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  ngAfterViewInit(): void {
    this.adjustInputWidth();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeServerSlot(): void {
    this.inputControl.setValue(this.serverSlot.name);
  }

  private initializeStatisticsSubscription(): void {
    this.serverStatisticsService.getLastServerStatistics(this.serverSlot.slotKey).pipe(
      tap(message => this.handleStatisticsMessage(message)),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  private handleStatisticsMessage(message: { key: string; statistics: ServerStatisticsResponse; } | null): void {
    if (!message || message.key !== this.serverSlot.slotKey) {
      return;
    }
    this.updateServerStatus(message.statistics);
  }

  onBlur(): void {
    if (this.inputIsEditable$.getValue()) {
      this.validateAndSaveInput();
    }
  }

  redirectToInfo(): void {
    this.redirector.redirectTo(`serverslot/${this.serverSlot.id}`);
  }

  showKey(): void {
    this.snackBarManager.openInfoSnackbar(`🔑: ${this.serverSlot.slotKey}`, 10);
  }

  private updateServerStatus(statistics: ServerStatisticsResponse): void {
    if (statistics.dataExists) {
      this.serverStatus$.next(statistics.isAlive ? ServerStatus.Online : ServerStatus.Offline);
    } else {
      this.serverStatus$.next(ServerStatus.NoData);
    }
  }

  private adjustInputWidth(): void {
    if (this.textSizer) {
      const sizer = this.textSizer.nativeElement;
      this.inputWidth$.next(Math.min(sizer.scrollWidth + 1, 400));
    }
  }

  private validateAndSaveInput(): void {
    this.checkEmptyInput();
    this.makeInputNonEditable();
    this.updateServerSlotName();
  }

  private checkEmptyInput(): void {
    if (!this.inputControl.value?.trim()) {
      this.inputControl.setValue('New slot');
      this.adjustInputWidth();
    }
  }

  makeInputEditable(): void {
    this.inputIsEditable$.next(true);
    setTimeout(() => {
      const inputElement = document.querySelector('input[name="slotName"]') as HTMLInputElement;
      inputElement.focus();
    });
  }

  private makeInputNonEditable(): void {
    this.inputIsEditable$.next(false);
  }

  openConfirmDeletion(): void {
    this.dialogManager.openDeleteSlotConfirmMenu().afterClosed().pipe(
      tap(result => {
        if (result === true) {
          this.serverSlotService.deleteServerSlot(this.serverSlot.id);
        }
      }),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  private updateServerSlotName(): void {
    const request: UpdateServerSlotRequest = {
      id: this.serverSlot.id,
      name: this.inputControl.value || '',
    };
    this.serverSlotService.updateServerSlot(request);
  }
}