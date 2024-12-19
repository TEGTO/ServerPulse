import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../../environment/environment';
import { getAuthData, selectAuthData, startLoginUser } from '../../../authentication';

@Component({
  selector: 'app-main-view',
  templateUrl: './main-view.component.html',
  styleUrls: ['./main-view.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainViewComponent implements OnInit {
  isAuthenticated$!: Observable<boolean>;
  projectUrl = environment.projectUrl;

  constructor(
    private readonly store: Store,
  ) { }

  ngOnInit(): void {
    this.store.dispatch(getAuthData());
    this.isAuthenticated$ = this.store.select(selectAuthData).pipe(
      map(data => data.isAuthenticated)
    );
  }

  startLogin() {
    this.store.dispatch(startLoginUser());
  }
}