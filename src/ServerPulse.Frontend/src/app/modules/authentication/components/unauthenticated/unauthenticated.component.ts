import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { getAuthData, startLoginUser } from '../..';

@Component({
  selector: 'app-unauthenticated',
  templateUrl: './unauthenticated.component.html',
  styleUrl: './unauthenticated.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UnauthenticatedComponent implements OnInit {
  constructor(
    private readonly store: Store,
  ) { }

  ngOnInit(): void {
    this.store.dispatch(getAuthData());
  }

  startLogin(): void {
    this.store.dispatch(startLoginUser());
  }
}
