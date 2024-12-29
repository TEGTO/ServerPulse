import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subject, of, throwError } from 'rxjs';
import { confirmEmail } from '../..';
import { RedirectorService } from '../../../shared';
import { EmailCallbackComponent } from './email-callback.component';

describe('EmailCallbackComponent', () => {
  let component: EmailCallbackComponent;
  let fixture: ComponentFixture<EmailCallbackComponent>;
  let routeSpy: jasmine.SpyObj<ActivatedRoute>;
  let redirectorSpy: jasmine.SpyObj<RedirectorService>;
  let storeSpy: jasmine.SpyObj<Store>;
  let destroy$: Subject<void>;

  beforeEach(async () => {
    routeSpy = jasmine.createSpyObj('ActivatedRoute', ['queryParams']);
    redirectorSpy = jasmine.createSpyObj('RedirectorService', ['redirectToHome']);
    storeSpy = jasmine.createSpyObj('Store', ['dispatch']);
    destroy$ = new Subject<void>();

    await TestBed.configureTestingModule({
      declarations: [EmailCallbackComponent],
      providers: [
        { provide: ActivatedRoute, useValue: routeSpy },
        { provide: RedirectorService, useValue: redirectorSpy },
        { provide: Store, useValue: storeSpy },
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EmailCallbackComponent);
    component = fixture.componentInstance;
    component.destroy$ = destroy$;
  });

  afterEach(() => {
    destroy$.next();
    destroy$.complete();
  });

  it('should dispatch confirmEmail action if token and email are present in query params', () => {
    const mockParams = { token: 'mock-token', email: 'test@example.com' };
    routeSpy.queryParams = of(mockParams);

    fixture.detectChanges();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(confirmEmail({
      req: { token: 'mock-token', email: 'test@example.com' }
    }));
    expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
  });

  it('should call redirectToHome when there is an error in queryParams', () => {
    routeSpy.queryParams = throwError(() => new Error('Mock Error'));

    fixture.detectChanges();

    expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
    expect(storeSpy.dispatch).not.toHaveBeenCalled();
  });

  it('should call redirectToHome even if token or email are missing', () => {
    const mockParams = { someKey: 'someValue' }; // Missing token and email
    routeSpy.queryParams = of(mockParams);

    fixture.detectChanges();

    expect(storeSpy.dispatch).not.toHaveBeenCalled();
    expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
  });

  it('should complete destroy$ when component is destroyed', () => {
    spyOn(destroy$, 'next');
    spyOn(destroy$, 'complete');

    component.ngOnDestroy();

    expect(destroy$.next).toHaveBeenCalled();
    expect(destroy$.complete).toHaveBeenCalled();
  });
});
