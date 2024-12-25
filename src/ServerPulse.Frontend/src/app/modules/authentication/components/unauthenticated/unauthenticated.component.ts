import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Store } from '@ngrx/store';
import { startLoginUser } from '../..';

@Component({
  selector: 'app-unauthenticated',
  templateUrl: './unauthenticated.component.html',
  styleUrl: './unauthenticated.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UnauthenticatedComponent {
  constructor(
    private readonly store: Store,
  ) { }

  startLogin(): void {
    this.store.dispatch(startLoginUser());
  }
}
