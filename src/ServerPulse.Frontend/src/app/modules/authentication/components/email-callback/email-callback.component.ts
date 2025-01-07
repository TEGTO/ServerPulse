import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { catchError, of, Subject, takeUntil } from 'rxjs';
import { confirmEmail, ConfirmEmailRequest } from '../..';
import { RedirectorService } from '../../../shared';

@Component({
  selector: 'app-email-callback',
  templateUrl: './email-callback.component.html',
  styleUrl: './email-callback.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmailCallbackComponent implements OnInit, OnDestroy {
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
        const token = params['token'];
        const email = params['email'];
        if (email) {
          const req: ConfirmEmailRequest =
          {
            email: email,
            token: token
          }
          this.store.dispatch(confirmEmail({ req: req }))
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