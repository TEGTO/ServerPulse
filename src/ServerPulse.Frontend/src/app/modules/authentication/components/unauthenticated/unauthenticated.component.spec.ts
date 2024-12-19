import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { startLoginUser } from '../..';
import { UnauthenticatedComponent } from './unauthenticated.component';

describe('UnauthenticatedComponent', () => {
  let component: UnauthenticatedComponent;
  let fixture: ComponentFixture<UnauthenticatedComponent>;
  let storeSpy: jasmine.SpyObj<Store>;

  beforeEach(async () => {
    // Mock the Store
    storeSpy = jasmine.createSpyObj<Store>(['dispatch']);

    await TestBed.configureTestingModule({
      declarations: [UnauthenticatedComponent],
      providers: [
        { provide: Store, useValue: storeSpy },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(UnauthenticatedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should dispatch startLoginUser action when startLogin is called', () => {
    const action = startLoginUser();

    component.startLogin();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(action);
  });

  it('should dispatch startLoginUser when the button is clicked', () => {
    const button = fixture.debugElement.nativeElement.querySelector('button');
    const action = startLoginUser();

    button.click();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(action);
  });
});
