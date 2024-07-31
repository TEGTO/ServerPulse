import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { environment } from '../../../../../environment/environment';
import { AuthenticationDialogManager, AuthenticationService } from '../../../authentication';

@Component({
  selector: 'app-main-view',
  templateUrl: './main-view.component.html',
  styleUrl: './main-view.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainViewComponent implements OnInit {
  isAuthenticated: boolean = false;
  projectUrl = environment.projectUrl;

  constructor(
    private readonly authService: AuthenticationService,
    private readonly authDialogManager: AuthenticationDialogManager,
    private readonly cdr: ChangeDetectorRef
  ) { }

  openLoginMenu() {
    const dialogRef = this.authDialogManager.openLoginMenu();
  }

  ngOnInit(): void {
    this.authService.getAuthData().subscribe(data => {
      this.isAuthenticated = data.isAuthenticated;
      this.cdr.detectChanges();
    })
  }
}