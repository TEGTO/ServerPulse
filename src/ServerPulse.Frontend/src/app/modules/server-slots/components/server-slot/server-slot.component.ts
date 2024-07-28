import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { ServerSlotDialogManager, ServerSlotService, ServerStatisticsService } from '../..';
import { RedirectorService, ServerSlot, ServerStatisticsResponse, SnackbarManager, UpdateServerSlotRequest } from '../../../shared';

enum ServerStatus {
  Online = 'green',
  Offline = 'red',
  NoData = 'grey'
}

@Component({
  selector: 'server-slot',
  templateUrl: './server-slot.component.html',
  styleUrl: './server-slot.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotComponent implements AfterViewInit, OnInit {
  @Input({ required: true }) serverSlot!: ServerSlot;
  @ViewChild('textSizer', { static: false }) textSizer!: ElementRef;
  @ViewChild('nameInput', { static: false }) nameInput!: ElementRef<HTMLInputElement>;
  updateRateFormControl = new FormControl('5', [Validators.required]);
  hideKey: boolean = true;
  inputIsEditable: boolean = false;
  serverStatus: ServerStatus = ServerStatus.NoData;
  inputWidth: number = 120;
  inputValue: string = "";
  private intervalId: any;
  private currentServerSlotStatistics: ServerStatisticsResponse | undefined;

  constructor(
    private readonly serverSlotService: ServerSlotService,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialogManager: ServerSlotDialogManager,
    private readonly redirector: RedirectorService,
    private readonly snackBarManager: SnackbarManager,
    private readonly serverStatisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    this.inputValue = this.serverSlot.name;
    this.fetchStatistics();
    this.startInterval();
    this.updateRateFormControl.valueChanges.subscribe(value => {
      if (value === 'DontUpdate') {
        this.stopInterval();
      } else {
        this.resetInterval();
      }
    });
  }
  ngAfterViewInit() {
    this.adjustInputWidth();
    this.cdr.detectChanges();
  }

  redirectToInfo() {
    this.redirector.redirectTo(`serverslot/${this.serverSlot.id}`);
  }

  showKey() {
    this.snackBarManager.openInfoSnackbar(`🔑: ${this.serverSlot.slotKey}`, 10);
  }

  onInputChange() {
    this.adjustInputWidth();
  }
  onBlur() {
    this.checkEmptyInput();
    this.makeInputNonEditable();
    this.updateServerSlotName();
  }

  private startInterval() {
    let updateInterval = parseInt(this.updateRateFormControl.value!, 10);
    if (updateInterval) {
      this.intervalId = setInterval(() => {
        this.fetchStatistics();
      }, updateInterval * 1000);
    }
  }
  private fetchStatistics() {
    this.serverStatisticsService.getCurrentServerStatisticsByKey(this.serverSlot.slotKey).subscribe(statistics => {
      this.currentServerSlotStatistics = statistics;
      this.toggleServerStatus();
      this.cdr.markForCheck();
    })
  }
  private stopInterval() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }
  private resetInterval() {
    this.stopInterval();
    this.startInterval();
  }
  toggleServerStatus() {
    if (this.currentServerSlotStatistics?.dataExists) {
      if (this.currentServerSlotStatistics?.isAlive) {
        this.serverStatus = ServerStatus.Online;
      }
      else {
        this.serverStatus = ServerStatus.Offline;
      }
    }
    else {
      this.serverStatus = ServerStatus.NoData;
    }
  }

  private adjustInputWidth() {
    const sizer = this.textSizer.nativeElement;
    this.inputWidth = Math.min(sizer.scrollWidth, 400);
  }
  private checkEmptyInput() {
    if (!this.inputValue.trim()) {
      this.inputValue = 'New slot';
      this.cdr.detectChanges();
      this.adjustInputWidth();
    }
  }
  makeInputEditable() {
    this.inputIsEditable = true;
    setTimeout(() => {
      this.nameInput.nativeElement.focus();
      this.cdr.detectChanges();
    });
  }
  private makeInputNonEditable() {
    this.inputIsEditable = false;
  }

  openConfirmDeletion() {
    this.dialogManager.openDeleteSlotConfirmMenu().afterClosed().subscribe(result => {
      if (result === true) {
        this.serverSlotService.deleteServerSlot(this.serverSlot.id);
      }
    });
  }

  private updateServerSlotName() {
    let request: UpdateServerSlotRequest =
    {
      id: this.serverSlot.id,
      name: this.inputValue
    }
    this.serverSlotService.updateServerSlot(request);
  }
}