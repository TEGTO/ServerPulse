import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subject, of, throwError } from 'rxjs';
import { oauthLogin } from '../..';
import { RedirectorService } from '../../../shared';
import { OAuthCallbackComponent } from './oauth-callback.component';

describe('OAuthCallbackComponent', () => {
  let component: OAuthCallbackComponent;
  let fixture: ComponentFixture<OAuthCallbackComponent>;
  let routeSpy: jasmine.SpyObj<ActivatedRoute>;
  let redirectorSpy: jasmine.SpyObj<RedirectorService>;
  let storeSpy: jasmine.SpyObj<Store>;
  let destroy$: Subject<void>;

  beforeEach(async () => {
    routeSpy = jasmine.createSpyObj('ActivatedRoute', ['queryParams']);
    storeSpy = jasmine.createSpyObj('Store', ['dispatch']);
    redirectorSpy = jasmine.createSpyObj('RedirectorService', ['redirectToHome']);
    destroy$ = new Subject<void>();

    await TestBed.configureTestingModule({
      declarations: [OAuthCallbackComponent],
      providers: [
        { provide: ActivatedRoute, useValue: routeSpy },
        { provide: RedirectorService, useValue: redirectorSpy },
        { provide: Store, useValue: storeSpy },
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OAuthCallbackComponent);
    component = fixture.componentInstance;
    component.destroy$ = destroy$;
  });

  afterEach(() => {
    destroy$.next();
    destroy$.complete();
  });

  it('should dispatch OAuthLoginCommand if code is present in query params', () => {
    const mockParams = { code: 'mock-code' };
    routeSpy.queryParams = of(mockParams);

    fixture.detectChanges();
    expect(storeSpy.dispatch).toHaveBeenCalledWith(oauthLogin({ code: 'mock-code' }));
    expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
  });

  it('should call redirectToHome when there is an error', () => {
    const error = new Error('Mock Error');
    routeSpy.queryParams = throwError(() => error);;

    fixture.detectChanges();

    expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
    expect(storeSpy.dispatch).not.toHaveBeenCalled();
  });

  it('should call redirectToHome after dispatching OAuthLoginCommand', () => {
    const mockParams = { code: 'mock-code' };
    routeSpy.queryParams = of(mockParams);

    fixture.detectChanges();

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
