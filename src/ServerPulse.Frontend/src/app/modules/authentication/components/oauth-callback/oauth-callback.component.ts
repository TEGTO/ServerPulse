import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { catchError, of, Subject, takeUntil } from 'rxjs';
import { oauthLogin } from '../..';
import { RedirectorService } from '../../../shared';

@Component({
  selector: 'app-oauth-callback',
  templateUrl: './oauth-callback.component.html',
  styleUrl: './oauth-callback.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OAuthCallbackComponent implements OnInit, OnDestroy {
  destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly redirector: RedirectorService,
    private readonly store: Store
  ) { }

  ngOnInit(): void {
    this.route.queryParams
      .pipe(
        takeUntil(this.destroy$),
        catchError((error) => {
          console.error('Error:', error);
          this.redirector.redirectToHome();
          return of();
        })
      )
      .subscribe(params => {
        const code = params['code'];
        if (code) {
          this.store.dispatch(oauthLogin({ code: code }));
        }
        this.redirector.redirectToHome();
        return of();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }
}