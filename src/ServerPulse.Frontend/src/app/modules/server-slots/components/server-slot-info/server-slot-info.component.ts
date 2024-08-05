import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ServerSlotDialogManager, ServerSlotService } from '../..';
import { ServerSlot, ServerStatisticsResponse, SnackbarManager, UpdateServerSlotRequest } from '../../../shared';
import { ServerStatus } from '../server-slot/server-slot.component';

@Component({
  selector: 'app-server-slot-info',
  templateUrl: './server-slot-info.component.html',
  styleUrl: './server-slot-info.component.scss',
})
export class ServerSlotInfoComponent implements OnInit {
  @ViewChild('textSizer', { static: false }) textSizer!: ElementRef;
  @ViewChild('nameInput', { static: false }) nameInput!: ElementRef<HTMLInputElement>;
  slotId: string | null = null;
  serverSlot!: ServerSlot;
  inputIsEditable: boolean = false;
  serverStatus: ServerStatus = ServerStatus.NoData;
  inputWidth: number = 120;
  inputValue: string = "";
  private currentServerSlotStatistics: ServerStatisticsResponse | undefined;

  constructor(
    private readonly serverSlotService: ServerSlotService,
    private readonly cdr: ChangeDetectorRef,
    private readonly snackBarManager: SnackbarManager,
    private readonly dialogManager: ServerSlotDialogManager,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.slotId = params.get('id');
      if (this.slotId) {
        this.serverSlotService.getServerSlotById(this.slotId).subscribe(slot => {
          if (slot) {
            this.serverSlot = slot;
            this.inputValue = this.serverSlot.name;
            this.cdr.detectChanges();
            this.adjustInputWidth();
          }
        })
      }
    });
  }

  onInputChange() {
    this.adjustInputWidth();
  }
  onBlur() {
    if (this.inputIsEditable) {
      this.checkEmptyInput();
      this.makeInputNonEditable();
      this.updateServerSlotName();
    }
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
    this.inputWidth = Math.min(sizer.scrollWidth, 300);
  }
  private checkEmptyInput() {
    if (!this.inputValue.trim()) {
      this.inputValue = 'New slot';
      this.adjustInputWidth();
    }
  }
  makeInputEditable() {
    this.inputIsEditable = true;
    setTimeout(() => {
      this.nameInput.nativeElement.focus();
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