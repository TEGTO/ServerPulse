import { Component, OnInit } from '@angular/core';
import { AuthenticationDialogManager, AuthenticationService } from '../../../authentication';

@Component({
  selector: 'app-main-view',
  templateUrl: './main-view.component.html',
  styleUrl: './main-view.component.scss'
})
export class MainViewComponent implements OnInit {
  isAuthenticated: boolean = false;

  constructor(private authService: AuthenticationService,
    private authDialogManager: AuthenticationDialogManager) { }

  openLoginMenu() {
    const dialogRef = this.authDialogManager.openLoginMenu();
  }

  ngOnInit(): void {
    this.authService.getAuthUserData().subscribe(data => {
      this.isAuthenticated = data.isAuthenticated;
    })
  }
}
