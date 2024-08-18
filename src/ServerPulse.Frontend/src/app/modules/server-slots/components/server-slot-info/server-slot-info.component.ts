import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, debounceTime, Subject, takeUntil, tap } from 'rxjs';
import { RedirectorService, ServerSlot, SnackbarManager, UpdateServerSlotRequest } from '../../../shared';
import { ServerSlotDialogManager, ServerSlotService, ServerStatisticsService } from '../../index';

@Component({
  selector: 'app-server-slot-info',
  templateUrl: './server-slot-info.component.html',
  styleUrls: ['./server-slot-info.component.scss'],
})
export class ServerSlotInfoComponent implements OnInit, OnDestroy {
  @ViewChild('textSizer', { static: false }) textSizer!: ElementRef;

  slotId$ = new BehaviorSubject<string | null>(null);
  serverSlot$ = new BehaviorSubject<ServerSlot | null>(null);
  inputIsEditable$ = new BehaviorSubject<boolean>(false);
  inputWidth$ = new BehaviorSubject<number>(120);
  inputControl = new FormControl('');
  private destroy$ = new Subject<void>();

  constructor(
    private readonly serverSlotService: ServerSlotService,
    private readonly statisticsService: ServerStatisticsService,
    private readonly snackBarManager: SnackbarManager,
    private readonly dialogManager: ServerSlotDialogManager,
    private readonly redirector: RedirectorService,
    private readonly route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const slotId = params.get('id');
        this.slotId$.next(slotId);

        if (slotId) {
          this.serverSlotService.getServerSlotById(slotId)
            .pipe(takeUntil(this.destroy$))
            .subscribe(slot => {
              if (slot) {
                this.serverSlot$.next(slot);
                this.inputControl.setValue(slot.name);
                this.adjustInputWidth();
              }
            });

          this.statisticsService.setCurrentLoadStatisticsDate(new Date());
        }
      });

    this.inputControl.valueChanges.pipe(
      debounceTime(300),
      tap(() => this.adjustInputWidth()),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onBlur() {
    this.checkEmptyInput();
    this.makeInputNonEditable();
    this.updateServerSlotName();
  }

  showKey() {
    const slot = this.serverSlot$.getValue();
    if (slot) {
      this.snackBarManager.openInfoSnackbar(`🔑: ${slot.slotKey}`, 10);
    }
  }

  private adjustInputWidth() {
    if (this.textSizer) {
      const sizer = this.textSizer?.nativeElement;
      this.inputWidth$.next(Math.min(sizer.scrollWidth + 1, 300));
    }
  }

  private checkEmptyInput() {
    if (!this.inputControl.value?.trim()) {
      this.inputControl.setValue('New slot');
      this.adjustInputWidth();
    }
  }

  makeInputEditable() {
    this.inputIsEditable$.next(true);
    setTimeout(() => {
      const inputElement = document.querySelector('input[name="slotName"]') as HTMLInputElement;
      inputElement.focus();
    });
  }

  private makeInputNonEditable() {
    this.inputIsEditable$.next(false);
  }

  openConfirmDeletion() {
    this.dialogManager.openDeleteSlotConfirmMenu().afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe(result => {
        if (result === true) {
          const slot = this.serverSlot$.getValue();
          if (slot) {
            this.serverSlotService.deleteServerSlot(slot.id);
            this.redirector.redirectToHome();
          }
        }
      });
  }

  private updateServerSlotName() {
    const slot = this.serverSlot$.getValue();
    if (slot) {
      const request: UpdateServerSlotRequest = {
        id: slot.id,
        name: this.inputControl.value || '',
      };
      this.serverSlotService.updateServerSlot(request);
    }
  }
}