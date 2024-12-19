import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { AuthenticatedComponent, LoginComponent, RegisterComponent } from '../..';
import { AuthenticationDialogManagerService } from './authentication-dialog-manager.service';

describe('AuthenticationDialogManagerService', () => {
    let service: AuthenticationDialogManagerService;
    let mockMatDialog: jasmine.SpyObj<MatDialog>

    beforeEach(() => {
        mockMatDialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

        TestBed.configureTestingModule({
            providers: [
                { provide: MatDialog, useValue: mockMatDialog },
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

    it('should open register dialog', () => {
        service.openRegisterMenu();

        expect(mockMatDialog.open).toHaveBeenCalledWith(RegisterComponent, {
            maxHeight: '455px',
            width: '500px',
        });
    });

    it('should open login dialog', () => {
        service.openLoginMenu();

        expect(mockMatDialog.open).toHaveBeenCalledWith(LoginComponent, {
            maxHeight: '455px',
            width: '500px',
        });
    });
});