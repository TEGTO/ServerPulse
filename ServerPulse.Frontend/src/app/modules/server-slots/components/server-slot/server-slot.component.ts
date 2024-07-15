import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { ServerSlotDialogManager, ServerSlotService } from '../..';
import { RedirectorService, ServerSlot, SnackbarManager, UpdateServerSlotRequest } from '../../../shared';

@Component({
  selector: 'server-slot',
  templateUrl: './server-slot.component.html',
  styleUrl: './server-slot.component.scss'
})
export class ServerSlotComponent implements AfterViewInit {
  @Input({ required: true }) serverSlot!: ServerSlot;
  updateRateFormControl = new FormControl('5', [Validators.required]);
  hideKey: boolean = true;
  inputValue: string = this.serverSlot.name;
  inputWidth: number = 120;
  inputIsEditable: boolean = false;
  @ViewChild('textSizer', { static: false }) textSizer!: ElementRef;
  @ViewChild('nameInput', { static: false }) nameInput!: ElementRef<HTMLInputElement>;

  constructor(
    private readonly serverSlotService: ServerSlotService,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialogManager: ServerSlotDialogManager,
    private readonly redirector: RedirectorService,
    private readonly snackBarManager: SnackbarManager
  ) { }

  redirectToInfo() {
    this.redirector.redirectTo(`serverslot/${this.serverSlot.id}`);
  }
  showKey() {
    this.snackBarManager.openInfoSnackbar(`🔑: ${this.serverSlot.slotKey}`, 10);
  }
  ngAfterViewInit() {
    this.adjustInputWidth();
    this.cdr.detectChanges();
  }
  onInputChange() {
    this.adjustInputWidth();
    this.updateServerSlotName();
  }
  onBlur() {
    this.checkEmptyInput();
    this.makeInputNonEditable();
    this.updateServerSlotName();
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
      if (result === 'true') {
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
