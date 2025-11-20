import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  imports: [CommonModule, FormsModule]
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit(): void {
    this.error = '';
    this.authService.login(this.email, this.password).subscribe({
      next: (res: any) => {
        // res kannst du später genauer typisieren (z. B. LoginResponse)
        this.router.navigate(['/accounts']);
      },
      error: (err: any) => {
        console.error(err);
        this.error = 'Login fehlgeschlagen. Bitte Daten prüfen.';
      }
    });
  }
}
