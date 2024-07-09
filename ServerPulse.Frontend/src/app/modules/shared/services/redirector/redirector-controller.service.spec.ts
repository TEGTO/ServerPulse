import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { RedirectorContollerService } from './redirector-controller.service';

describe('RedirectorContollerService', () => {
  var mockRouter: jasmine.SpyObj<Router>;
  var service: RedirectorContollerService;

  beforeEach(() => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    const activatedRouteMock = jasmine.createSpyObj('ActivatedRoute', [], {
      snapshot: {
        paramMap: jasmine.createSpyObj('ParamMap', ['get'])
      }
    });

    TestBed.configureTestingModule({
      providers: [
        RedirectorContollerService,
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ]
    });
    service = TestBed.inject(RedirectorContollerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should navigate to home', () => {
    service.redirectToHome();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['']);
  });

  it('should navigate to some path', () => {
    let somePath = "some path";
    service.redirectTo(somePath);
    expect(mockRouter.navigate).toHaveBeenCalledWith([somePath]);
  });
});
