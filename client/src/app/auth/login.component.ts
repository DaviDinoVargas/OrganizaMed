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
  if (this.form.invalid) {
    this.snack.open('Preencha usuário e senha.', 'Fechar', { duration: 3000 });
    return;
  }

  this.loading = true;
  const { userName, password } = this.form.value;

  console.log('Tentando autenticar', { userName });

  this.auth.autenticar(userName!, password!).subscribe({
    next: (tokenResp) => {
      console.log('Resposta do /autenticar:', tokenResp);

      // Verifica token/usúario conforme seu AuthService (ajuste se os nomes de storage mudaram)
      const token = this.auth.getToken();
      const usuario = this.auth.getUsuario();
      const isLogged = this.auth.isLoggedIn();

      console.log('getToken():', token);
      console.log('getUsuario():', usuario);
      console.log('isLoggedIn():', isLogged);

      if (!token) {
        this.loading = false;
        this.snack.open('Login recebido, mas token ausente. Verifique servidor.', 'Fechar', { duration: 6000 });
        return;
      }

      // tenta navegar explicitamente e loga o resultado
      this.router.navigateByUrl(this.returnUrl).then(navSucceeded => {
        this.loading = false;
        console.log('router.navigateByUrl result:', navSucceeded);
        if (!navSucceeded) {
          this.snack.open('Navegação falhou (guard ou rota). Verifique console.', 'Fechar', { duration: 5000 });
        }
      }).catch(err => {
        this.loading = false;
        console.error('Erro no router.navigateByUrl:', err);
        this.snack.open('Erro ao navegar: veja o console.', 'Fechar', { duration: 5000 });
      });
    },
    error: (err) => {
      this.loading = false;
      console.error('Erro ao autenticar:', err);
      const msg = err?.error && (typeof err.error === 'string' ? err.error : JSON.stringify(err.error));
      this.snack.open(msg ?? 'Falha no login. Verifique usuário/senha.', 'Fechar', { duration: 5000 });
    }
  });
}
}
