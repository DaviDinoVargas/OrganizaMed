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

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/';
  }

  submit() {
  if (this.form.invalid) return;
  this.loading = true;
  const { userName, email, password } = this.form.value;
  this.auth.registrar(userName!, email!, password!).subscribe({
    next: (res) => {
      this.snack.open('Conta criada e logado.', 'Ok', { duration: 2500 });
      this.router.navigate(['/']);
    },
    error: (err) => {
      this.loading = false;
      // err.error deve conter a lista de mensagens retornadas pelo FluentResults
      const mensagens = Array.isArray(err.error) ? err.error.join('\n') : 'Falha ao registrar. Confira os dados.';
      this.snack.open(mensagens, 'Fechar', { duration: 4000 });
    }
  });
}
}
