import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { ServerSlotDialogManager, ServerSlotService, ServerStatisticsService } from '../..';
import { convertToServerStatisticsResponse, RedirectorService, ServerSlot, ServerStatisticsResponse, SnackbarManager, UpdateServerSlotRequest } from '../../../shared';

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
  hideKey: boolean = true;
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
    private readonly serverStatisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    this.inputValue = this.serverSlot.name;
    this.serverStatisticsService.startConnection().subscribe(() => {
      this.serverStatisticsService.startListenPulse(this.serverSlot.slotKey);
      this.serverStatisticsService.receiveStatistics().subscribe(
        (message) => {
          try {
            if (message.key === this.serverSlot.slotKey) {
              this.currentServerSlotStatistics = convertToServerStatisticsResponse(JSON.parse(message.data));
              this.toggleServerStatus();
              this.cdr.detectChanges();
            }
          } catch (error) {
            this.currentServerSlotStatistics = undefined;
            console.error('Error processing the received statistics:', error);
          }
        },
        (error) => {
          this.currentServerSlotStatistics = undefined;
          console.error('Error receiving statistics:', error);
        }
      );
    });
  }
  ngAfterViewInit() {
    this.adjustInputWidth();
    this.cdr.detectChanges();
  }

  onInputChange() {
    this.adjustInputWidth();
  }
  onBlur() {
    this.checkEmptyInput();
    this.makeInputNonEditable();
    this.updateServerSlotName();
  }

  redirectToInfo() {
    this.redirector.redirectTo(`serverslot/${this.serverSlot.id}`);
  }
  showKey() {
    this.snackBarManager.openInfoSnackbar(`🔑: ${this.serverSlot.slotKey}`, 10);
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