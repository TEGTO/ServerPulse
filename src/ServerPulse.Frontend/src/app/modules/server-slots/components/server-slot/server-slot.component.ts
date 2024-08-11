import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { ServerSlotDialogManager, ServerSlotService, ServerStatisticsService } from '../..';
import { RedirectorService, ServerSlot, ServerStatisticsResponse, SnackbarManager, UpdateServerSlotRequest } from '../../../shared';

export enum ServerStatus {
  Online = 'green',
  Offline = 'red',
  NoData = 'grey'
}

@Component({
  selector: 'server-slot',
  templateUrl: './server-slot.component.html',
  styleUrls: ['./server-slot.component.scss'],
})
export class ServerSlotComponent implements AfterViewInit, OnInit {
  @Input({ required: true }) serverSlot!: ServerSlot;
  @ViewChild('textSizer', { static: false }) textSizer!: ElementRef;
  @ViewChild('nameInput', { static: false }) nameInput!: ElementRef<HTMLInputElement>;

  inputIsEditable: boolean = false;
  serverStatus: ServerStatus = ServerStatus.NoData;
  inputWidth: number = 120;
  inputValue: string = "";
  private currentServerSlotStatistics: ServerStatisticsResponse | undefined;

  constructor(
    private readonly serverSlotService: ServerSlotService,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialogManager: ServerSlotDialogManager,
    private readonly redirector: RedirectorService,
    private readonly snackBarManager: SnackbarManager,
    private readonly serverStatisticsService: ServerStatisticsService,
  ) { }

  ngOnInit(): void {
    this.initializeServerSlot();
    this.initializeStatisticsSubscription();
  }

  ngAfterViewInit(): void {
    this.adjustInputWidth();
  }

  private initializeServerSlot(): void {
    this.inputValue = this.serverSlot.name;
  }

  private initializeStatisticsSubscription(): void {
    this.serverStatisticsService.getLastServerStatistics(this.serverSlot.slotKey).subscribe(message => {
      this.handleStatisticsMessage(message);
    });
  }

  private handleStatisticsMessage(message: { key: string; statistics: ServerStatisticsResponse; } | null): void {
    if (!message || message.key !== this.serverSlot.slotKey) {
      return;
    }
    this.currentServerSlotStatistics = message.statistics;
    this.updateServerStatus();
    this.cdr.detectChanges();
  }

  onInputChange(): void {
    this.adjustInputWidth();
  }

  onBlur(): void {
    if (this.inputIsEditable) {
      this.validateAndSaveInput();
    }
  }

  redirectToInfo(): void {
    this.redirector.redirectTo(`serverslot/${this.serverSlot.id}`);
  }

  showKey(): void {
    this.snackBarManager.openInfoSnackbar(`🔑: ${this.serverSlot.slotKey}`, 10);
  }

  private updateServerStatus(): void {
    if (this.currentServerSlotStatistics?.dataExists) {
      this.serverStatus = this.currentServerSlotStatistics.isAlive ? ServerStatus.Online : ServerStatus.Offline;
    } else {
      this.serverStatus = ServerStatus.NoData;
    }
  }

  private adjustInputWidth(): void {
    const sizer = this.textSizer.nativeElement;
    this.inputWidth = Math.min(sizer.scrollWidth, 400);
  }

  private validateAndSaveInput(): void {
    this.checkEmptyInput();
    this.makeInputNonEditable();
    this.updateServerSlotName();
  }

  private checkEmptyInput(): void {
    if (!this.inputValue.trim()) {
      this.inputValue = 'New slot';
      this.adjustInputWidth();
    }
  }

  makeInputEditable(): void {
    this.inputIsEditable = true;
    setTimeout(() => {
      this.nameInput.nativeElement.focus();
    });
  }

  private makeInputNonEditable(): void {
    this.inputIsEditable = false;
  }

  openConfirmDeletion(): void {
    this.dialogManager.openDeleteSlotConfirmMenu().afterClosed().subscribe(result => {
      if (result === true) {
        this.serverSlotService.deleteServerSlot(this.serverSlot.id);
      }
    });
  }

  private updateServerSlotName(): void {
    const request: UpdateServerSlotRequest = {
      id: this.serverSlot.id,
      name: this.inputValue
    };
    this.serverSlotService.updateServerSlot(request);
  }
}