// src/app/auth/login.component.ts
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
    MatProgressSpinnerModule
  ],
  templateUrl: './login.component.html',
  // styleUrls: ['./login.component.css'] // opcional
})
export class LoginComponent {
  form!: FormGroup;           // <-- declare aqui (definite assignment)
  returnUrl = '/';
  loading = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private snack: MatSnackBar
  ) {
    // inicializa o form **depois** do FormBuilder estar disponível
    this.form = this.fb.group({
      userName: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/home';
  }

 submit() {
  if (this.form.invalid) return;
  this.loading = true;

  const { userName, password } = this.form.value;

  // Chamada correta para autenticação
  this.auth.autenticar(userName!, password!).subscribe({
    next: () => {
      this.snack.open('Login realizado com sucesso', 'Ok', { duration: 2000 });
      this.router.navigateByUrl(this.returnUrl);
    },
    error: () => {
      this.loading = false;
      this.snack.open('Falha no login. Verifique usuário/senha.', 'Fechar', { duration: 4000 });
    }
  });
}
}
