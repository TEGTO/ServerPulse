/* eslint-disable @typescript-eslint/no-explicit-any */
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Inject, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_SNACK_BAR_DATA, MatSnackBarAction, MatSnackBarRef } from '@angular/material/snack-bar';

@Component({
  selector: 'app-info-copy-annotated',
  standalone: true,
  imports: [MatButtonModule, MatSnackBarAction, CommonModule],
  templateUrl: './info-copy-annotated.component.html',
  styleUrl: './info-copy-annotated.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InfoCopyAnnotatedComponent {
  snackBarRef = inject(MatSnackBarRef);

  get message() { return this.data.message; }
  get copyMessage() { return this.data.copyMessage; }

  constructor(
    @Inject(MAT_SNACK_BAR_DATA) private readonly data: any
  ) { }

  copyToClipboard() {
    navigator.clipboard.writeText(this.copyMessage).then(() => {
      this.snackBarRef.dismissWithAction()
    }).catch(err => {
      console.error('Failed to copy text: ', err);
    });
  }
}
