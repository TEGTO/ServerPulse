import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { RedirectorService } from './redirector.service';

@Injectable({
    providedIn: 'root'
})
export class RedirectorContollerService implements RedirectorService {

    constructor(private router: Router) { }

    redirectToHome(): void {
        this.router.navigate(['']);
    }
    redirectTo(path: string): void {
        this.router.navigate([path]);
    }
}
