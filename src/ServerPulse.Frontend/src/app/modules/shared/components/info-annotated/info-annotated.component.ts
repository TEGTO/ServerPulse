import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_SNACK_BAR_DATA, MatSnackBarAction, MatSnackBarActions, MatSnackBarLabel, MatSnackBarRef } from '@angular/material/snack-bar';

@Component({
  selector: 'app-info-annotated',
  standalone: true,
  imports: [MatButtonModule, MatSnackBarLabel, MatSnackBarActions, MatSnackBarAction, CommonModule],
  templateUrl: './info-annotated.component.html',
  styleUrl: './info-annotated.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InfoAnnotatedComponent {
  snackBarRef = inject(MatSnackBarRef);

  get message() { return this.data.message; }

  constructor(
    @Inject(MAT_SNACK_BAR_DATA) private data: any
  ) { }
}
