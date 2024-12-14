import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { AuthenticatedComponent, AuthenticationService, LoginComponent, RegisterComponent } from '../..';
import { DialogManagerService, getDefaultUserAuth, UserAuth } from '../../../shared';
import { AuthenticationDialogManagerService } from './authentication-dialog-manager.service';

describe('AuthenticationDialogManagerService', () => {
    const mockUserAuth: UserAuth =
    {
        ...getDefaultUserAuth(),
        isAuthenticated: true,
    }

    let service: AuthenticationDialogManagerService;
    let mockMatDialog: jasmine.SpyObj<MatDialog>
    let mockAuthService: jasmine.SpyObj<AuthenticationService>
    let mockDialogManager: jasmine.SpyObj<DialogManagerService>

    beforeEach(() => {
        mockMatDialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
        mockAuthService = jasmine.createSpyObj<AuthenticationService>('AuthenticationService', ['getUserAuth']);
        mockDialogManager = jasmine.createSpyObj<DialogManagerService>(['openConfirmMenu']);
        mockAuthService.getUserAuth.and.returnValue(of(mockUserAuth));

        TestBed.configureTestingModule({
            providers: [
                { provide: MatDialog, useValue: mockMatDialog },
                { provide: AuthenticationService, useValue: mockAuthService },
                { provide: DialogManagerService, useValue: mockDialogManager }
            ],
        });
        service = TestBed.inject(AuthenticationDialogManagerService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should open authenticated dialog', () => {
        service.openAuthenticatedMenu();

        expect(mockMatDialog.open).toHaveBeenCalledWith(AuthenticatedComponent, {
            maxHeight: '440px',
            width: '450px',
        });
    });

    it('should open login dialog', () => {
        const mockUserAuth: UserAuth =
        {
            ...getDefaultUserAuth(),
            isAuthenticated: false,
        }
        mockAuthService.getUserAuth.and.returnValue(of(mockUserAuth));
        service = new AuthenticationDialogManagerService(mockAuthService, mockMatDialog, mockDialogManager);

        service.openLoginMenu();

        expect(mockMatDialog.open).toHaveBeenCalledWith(LoginComponent, {
            maxHeight: '455px',
            width: '500px',
        });
    });

    it('should open register dialog', () => {
        service.openRegisterMenu();

        expect(mockMatDialog.open).toHaveBeenCalledWith(RegisterComponent, {
            maxHeight: '455px',
            width: '500px',
        });
    });

});