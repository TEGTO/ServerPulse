import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { SnackbarManager } from '../../../shared';

@Component({
  selector: 'app-server-slot-edit',
  templateUrl: './server-slot-edit.component.html',
  styleUrl: './server-slot-edit.component.scss'
})
export class ServerSlotEditComponent {
  formGroup: FormGroup = null!;
  isUpdateSuccessful: boolean = false;

  get nameInput() { return this.formGroup.get('name')!; }

  constructor(private snackbarManager: SnackbarManager) {
  }

  ngOnInit(): void {
    this.formGroup = new FormGroup(
      {
        name: new FormControl("", [Validators.required, Validators.maxLength(256)]),
      });
  }

  updateSlot() {
    if (this.formGroup.valid) {
      const formValues = { ...this.formGroup.value };
    }
  }
}
