import { Component } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from './auth.service';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon'; // adicione
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatSnackBarModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    FormsModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['CSS/auth-shared-styles.css']
})
export class LoginComponent {
  form!: FormGroup;
  returnUrl = '/';
  loading = false;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private snack: MatSnackBar
  ) {
    this.form = this.fb.group({
      userName: ['', Validators.required],
      password: ['', Validators.required]
    });
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/home';
  }

  toggleShowPassword(): void {
    this.showPassword = !this.showPassword;
  }

  submit() {
    if (this.form.invalid) {
      this.snack.open('Preencha usuário e senha.', 'Fechar', { duration: 3000 });
      return;
    }

    this.loading = true;
    const { userName, password } = this.form.value;

    this.auth.autenticar(userName!, password!).subscribe({
      next: (tokenResp) => {
        this.loading = false;
        this.router.navigateByUrl(this.returnUrl).catch(() => {});
      },
      error: (err) => {
        this.loading = false;
        this.snack.open('Falha no login. Verifique usuário/senha.', 'Fechar', { duration: 5000 });
      }
    });
  }
}
