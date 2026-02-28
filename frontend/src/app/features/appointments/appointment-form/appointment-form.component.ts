import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatRadioModule } from '@angular/material/radio';
import { AppointmentService } from '../../../core/services/appointment.service';
import { CustomerService } from '../../../core/services/customer.service';
import { UserService } from '../../../core/services/user.service';
import { ServiceService } from '../../../core/services/service.service';
import { VacancyService } from '../../../core/services/vacancy.service';
import { AuthService } from '../../../core/services/auth.service';
import { CustomerDto } from '../../../core/models/customer.model';
import { UserDto } from '../../../core/models/user.model';
import { ServiceDto } from '../../../core/models/service.model';
import { VacancyDto } from '../../../core/models/vacancy.model';
import { CurrencyCentsPipe } from '../../../shared/pipes/currency-cents.pipe';
import { DateFormatPipe } from '../../../shared/pipes/date-format.pipe';

@Component({
  selector: 'app-appointment-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatSelectModule, MatIconModule, MatStepperModule,
    MatRadioModule, CurrencyCentsPipe, DateFormatPipe,
  ],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>Novo Agendamento</h2>
      <p class="dialog-subtitle">Siga os passos para criar um agendamento</p>
    </div>
    <mat-dialog-content>
      <mat-stepper linear #stepper>
        <!-- Step 1: Cliente -->
        <mat-step [stepControl]="clientForm" label="Cliente">
          <form [formGroup]="clientForm" class="step-content">
            <div class="step-description">Selecione um cliente existente ou cadastre um novo</div>
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Cliente</mat-label>
              <mat-icon matPrefix>person</mat-icon>
              <mat-select formControlName="customerId">
                @for (c of customers; track c.id) {
                  <mat-option [value]="c.id">{{ c.fullName }} — {{ c.email }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            @if (!showNewCustomer) {
              <button mat-stroked-button type="button" (click)="showNewCustomer = true" class="new-btn">
                <mat-icon>person_add</mat-icon> Novo Cliente
              </button>
            } @else {
              <div class="new-customer-form">
                <h4>Cadastrar Novo Cliente</h4>
                <mat-form-field class="full-width" appearance="outline">
                  <mat-label>Nome Completo</mat-label>
                  <input matInput formControlName="newCustomerName" />
                </mat-form-field>
                <mat-form-field class="full-width" appearance="outline">
                  <mat-label>CPF/CNPJ</mat-label>
                  <input matInput formControlName="newCustomerCpfCnpj" />
                </mat-form-field>
                <mat-form-field class="full-width" appearance="outline">
                  <mat-label>Email</mat-label>
                  <input matInput type="email" formControlName="newCustomerEmail" />
                </mat-form-field>
                <mat-form-field class="full-width" appearance="outline">
                  <mat-label>Telefone</mat-label>
                  <input matInput formControlName="newCustomerPhone" />
                </mat-form-field>
                <div class="form-row">
                  <button mat-stroked-button type="button" (click)="showNewCustomer = false">Cancelar</button>
                  <button mat-flat-button color="primary" type="button" [disabled]="creatingCustomer"
                    (click)="createCustomer()">
                    {{ creatingCustomer ? 'Salvando...' : 'Cadastrar' }}
                  </button>
                </div>
              </div>
            }

            <div class="step-actions">
              <span></span>
              <button mat-flat-button color="primary" matStepperNext [disabled]="clientForm.get('customerId')?.invalid">
                Próximo <mat-icon>arrow_forward</mat-icon>
              </button>
            </div>
          </form>
        </mat-step>

        <!-- Step 2: Serviço + Profissional -->
        <mat-step [stepControl]="serviceForm" label="Serviço">
          <form [formGroup]="serviceForm" class="step-content">
            <div class="step-description">Escolha o serviço e o profissional</div>
            <div class="service-cards">
              @for (s of services; track s.id) {
                <div class="service-card" [class.selected]="serviceForm.get('serviceId')?.value === s.id"
                  (click)="selectService(s)">
                  <div class="service-card-name">{{ s.name }}</div>
                  <div class="service-card-meta">
                    <span>{{ s.durationMinutes }} min</span>
                    <span class="service-card-price">{{ s.priceInCents | currencyCents }}</span>
                  </div>
                  @if (s.description) {
                    <div class="service-card-desc">{{ s.description }}</div>
                  }
                </div>
              }
            </div>

            @if (serviceForm.get('serviceId')?.value) {
              <mat-form-field class="full-width" appearance="outline">
                <mat-label>Profissional</mat-label>
                <mat-icon matPrefix>badge</mat-icon>
                <mat-select formControlName="userId">
                  @for (u of users; track u.id) {
                    <mat-option [value]="u.id">{{ u.fullName }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
            }

            <div class="step-actions">
              <button mat-button matStepperPrevious><mat-icon>arrow_back</mat-icon> Voltar</button>
              <button mat-flat-button color="primary" matStepperNext
                [disabled]="serviceForm.invalid" (click)="loadVacancies()">
                Próximo <mat-icon>arrow_forward</mat-icon>
              </button>
            </div>
          </form>
        </mat-step>

        <!-- Step 3: Horário -->
        <mat-step [stepControl]="timeForm" label="Horário">
          <form [formGroup]="timeForm" class="step-content">
            <div class="step-description">Selecione uma vaga disponível ou informe o horário manualmente</div>

            @if (availableVacancies.length > 0) {
              <div class="vacancy-section">
                <h4>Vagas Disponíveis</h4>
                <div class="vacancy-cards">
                  @for (v of availableVacancies; track v.id) {
                    <div class="vacancy-card" [class.selected]="selectedVacancyId === v.id"
                      (click)="selectVacancy(v)">
                      <mat-icon>schedule</mat-icon>
                      <span>{{ v.startTime | dateFormat }} — {{ v.endTime | dateFormat:'time' }}</span>
                    </div>
                  }
                </div>
              </div>
              <div class="divider-text">ou informe manualmente</div>
            }

            <div class="form-row">
              <mat-form-field appearance="outline">
                <mat-label>Data e Hora Início</mat-label>
                <input matInput type="datetime-local" formControlName="startTime"
                  (change)="onStartTimeChange()" />
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Data e Hora Fim</mat-label>
                <input matInput type="datetime-local" formControlName="endTime" />
              </mat-form-field>
            </div>

            <div class="step-actions">
              <button mat-button matStepperPrevious><mat-icon>arrow_back</mat-icon> Voltar</button>
              <button mat-flat-button color="primary" matStepperNext [disabled]="timeForm.invalid">
                Próximo <mat-icon>arrow_forward</mat-icon>
              </button>
            </div>
          </form>
        </mat-step>

        <!-- Step 4: Confirmação -->
        <mat-step label="Confirmação">
          <div class="step-content">
            <div class="step-description">Revise os dados e confirme o agendamento</div>

            <div class="summary-card">
              <div class="summary-row">
                <span class="summary-label">Cliente</span>
                <span class="summary-value">{{ getSelectedCustomerName() }}</span>
              </div>
              <div class="summary-row">
                <span class="summary-label">Serviço</span>
                <span class="summary-value">{{ getSelectedServiceName() }}</span>
              </div>
              <div class="summary-row">
                <span class="summary-label">Profissional</span>
                <span class="summary-value">{{ getSelectedUserName() }}</span>
              </div>
              <div class="summary-row">
                <span class="summary-label">Horário</span>
                <span class="summary-value">{{ timeForm.get('startTime')?.value }} — {{ timeForm.get('endTime')?.value }}</span>
              </div>
              <div class="summary-row highlight">
                <span class="summary-label">Valor</span>
                <span class="summary-value">{{ selectedService?.priceInCents | currencyCents }}</span>
              </div>
            </div>

            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Observações</mat-label>
              <textarea matInput [(ngModel)]="notes" [ngModelOptions]="{standalone: true}" rows="2" placeholder="Notas opcionais..."></textarea>
            </mat-form-field>

            <div class="step-actions">
              <button mat-button matStepperPrevious><mat-icon>arrow_back</mat-icon> Voltar</button>
              <button mat-flat-button color="primary" [disabled]="saving" (click)="save()">
                <mat-icon>check</mat-icon>
                {{ saving ? 'Criando...' : 'Criar Agendamento' }}
              </button>
            </div>
          </div>
        </mat-step>
      </mat-stepper>
    </mat-dialog-content>
  `,
  styles: [`
    .dialog-header { padding: 24px 24px 0; }
    h2 { margin: 0; font-size: 20px; font-weight: 700; color: #111827; }
    .dialog-subtitle { margin: 4px 0 0; font-size: 14px; color: #6B7280; }
    .step-content { display: flex; flex-direction: column; gap: 8px; padding-top: 16px; }
    .step-description { font-size: 14px; color: #6B7280; margin-bottom: 8px; }
    .step-actions { display: flex; justify-content: space-between; margin-top: 16px; }
    .form-row { display: flex; gap: 12px; > * { flex: 1; } }

    .new-btn { align-self: flex-start; }
    .new-customer-form {
      background: #F9FAFB; border-radius: 12px; padding: 16px;
      border: 1px solid #E5E7EB;
      h4 { margin: 0 0 12px; font-size: 14px; font-weight: 600; color: #374151; }
    }

    .service-cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 12px; margin-bottom: 16px; }
    .service-card {
      padding: 16px; border-radius: 12px; border: 2px solid #E5E7EB; cursor: pointer;
      transition: all 0.2s;
      &:hover { border-color: #A5B4FC; background: #F5F3FF; }
      &.selected { border-color: #4F46E5; background: #EEF2FF; }
    }
    .service-card-name { font-weight: 600; font-size: 14px; color: #111827; }
    .service-card-meta { display: flex; justify-content: space-between; margin-top: 8px; font-size: 13px; color: #6B7280; }
    .service-card-price { font-weight: 600; color: #059669; }
    .service-card-desc { font-size: 12px; color: #9CA3AF; margin-top: 4px; }

    .vacancy-section { margin-bottom: 12px; h4 { font-size: 14px; font-weight: 600; color: #374151; margin: 0 0 8px; } }
    .vacancy-cards { display: flex; flex-wrap: wrap; gap: 8px; }
    .vacancy-card {
      display: flex; align-items: center; gap: 8px; padding: 10px 16px;
      border-radius: 10px; border: 2px solid #E5E7EB; cursor: pointer;
      font-size: 13px; transition: all 0.2s;
      mat-icon { font-size: 18px; width: 18px; height: 18px; color: #6B7280; }
      &:hover { border-color: #A5B4FC; background: #F5F3FF; }
      &.selected { border-color: #4F46E5; background: #EEF2FF; mat-icon { color: #4F46E5; } }
    }
    .divider-text { text-align: center; color: #9CA3AF; font-size: 13px; margin: 12px 0; position: relative;
      &::before, &::after { content: ''; position: absolute; top: 50%; width: 40%; height: 1px; background: #E5E7EB; }
      &::before { left: 0; } &::after { right: 0; }
    }

    .summary-card {
      background: #F9FAFB; border-radius: 12px; padding: 16px; border: 1px solid #E5E7EB;
      display: flex; flex-direction: column; gap: 12px; margin-bottom: 16px;
    }
    .summary-row { display: flex; justify-content: space-between; font-size: 14px; }
    .summary-label { color: #6B7280; }
    .summary-value { font-weight: 500; color: #111827; }
    .summary-row.highlight { padding-top: 12px; border-top: 1px solid #E5E7EB;
      .summary-value { font-size: 18px; font-weight: 700; color: #059669; }
    }
  `],
})
export class AppointmentFormComponent implements OnInit {
  dialogRef = inject(MatDialogRef<AppointmentFormComponent>);
  private fb = inject(FormBuilder);
  private appointmentService = inject(AppointmentService);
  private customerService = inject(CustomerService);
  private userService = inject(UserService);
  private serviceService = inject(ServiceService);
  private vacancyService = inject(VacancyService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  customers: CustomerDto[] = [];
  users: UserDto[] = [];
  services: ServiceDto[] = [];
  availableVacancies: VacancyDto[] = [];
  selectedService: ServiceDto | null = null;
  selectedVacancyId: string | null = null;
  showNewCustomer = false;
  creatingCustomer = false;
  saving = false;
  notes = '';

  clientForm = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    newCustomerName: [''],
    newCustomerEmail: [''],
    newCustomerPhone: [''],
    newCustomerCpfCnpj: [''],
  });

  serviceForm = this.fb.nonNullable.group({
    serviceId: ['', Validators.required],
    userId: ['', Validators.required],
  });

  timeForm = this.fb.nonNullable.group({
    startTime: ['', Validators.required],
    endTime: ['', Validators.required],
  });

  ngOnInit(): void {
    this.customerService.getAll(undefined, 1, 100).subscribe({
      next: (result) => { this.customers = result.items; },
    });

    const currentUser = this.authService.currentUser();
    this.userService.getAll().subscribe({
      next: (users) => {
        this.users = users.filter(u => u.isActive);
        if (currentUser && this.users.some(u => u.id === currentUser.userId)) {
          this.serviceForm.patchValue({ userId: currentUser.userId });
        }
      },
      error: () => {
        if (currentUser) {
          this.users = [{
            id: currentUser.userId, fullName: currentUser.fullName,
            email: '', role: currentUser.role, isActive: true, createdAt: '',
          }];
          this.serviceForm.patchValue({ userId: currentUser.userId });
        }
      },
    });

    this.serviceService.getAll().subscribe({
      next: (services) => { this.services = services.filter(s => s.isActive); },
    });
  }

  createCustomer(): void {
    const name = this.clientForm.get('newCustomerName')?.value;
    const email = this.clientForm.get('newCustomerEmail')?.value;
    const phone = this.clientForm.get('newCustomerPhone')?.value;
    const cpfCnpj = this.clientForm.get('newCustomerCpfCnpj')?.value;
    if (!name || !email || !cpfCnpj) return;
    this.creatingCustomer = true;
    this.customerService.create({ fullName: name, email, phone: phone || undefined, cpfCnpj }).subscribe({
      next: (customer) => {
        this.customers = [...this.customers, customer];
        this.clientForm.patchValue({ customerId: customer.id });
        this.showNewCustomer = false;
        this.creatingCustomer = false;
        this.snackBar.open('Cliente cadastrado!', 'OK', { duration: 2000, panelClass: ['snackbar-success'] });
      },
      error: () => { this.creatingCustomer = false; },
    });
  }

  selectService(service: ServiceDto): void {
    this.selectedService = service;
    this.serviceForm.patchValue({ serviceId: service.id });
  }

  loadVacancies(): void {
    const userId = this.serviceForm.get('userId')?.value;
    const serviceId = this.serviceForm.get('serviceId')?.value;
    if (!userId || !serviceId) return;
    this.vacancyService.getAll({ userId, serviceId, available: true }).subscribe({
      next: (vacancies) => { this.availableVacancies = vacancies; },
    });
  }

  selectVacancy(vacancy: VacancyDto): void {
    this.selectedVacancyId = vacancy.id;
    const start = new Date(vacancy.startTime);
    const end = new Date(vacancy.endTime);
    const toLocal = (d: Date) => {
      const offset = d.getTimezoneOffset();
      const local = new Date(d.getTime() - offset * 60000);
      return local.toISOString().slice(0, 16);
    };
    this.timeForm.patchValue({ startTime: toLocal(start), endTime: toLocal(end) });
  }

  onStartTimeChange(): void {
    if (this.selectedService && this.timeForm.get('startTime')?.value) {
      const start = new Date(this.timeForm.get('startTime')!.value);
      const end = new Date(start.getTime() + this.selectedService.durationMinutes * 60000);
      const offset = end.getTimezoneOffset();
      const local = new Date(end.getTime() - offset * 60000);
      this.timeForm.patchValue({ endTime: local.toISOString().slice(0, 16) });
      this.selectedVacancyId = null;
    }
  }

  getSelectedCustomerName(): string {
    const id = this.clientForm.get('customerId')?.value;
    return this.customers.find(c => c.id === id)?.fullName ?? '';
  }

  getSelectedServiceName(): string {
    return this.selectedService?.name ?? '';
  }

  getSelectedUserName(): string {
    const id = this.serviceForm.get('userId')?.value;
    return this.users.find(u => u.id === id)?.fullName ?? '';
  }

  save(): void {
    this.saving = true;
    const startVal = this.timeForm.get('startTime')!.value;
    const endVal = this.timeForm.get('endTime')!.value;

    this.appointmentService.create({
      customerId: this.clientForm.get('customerId')!.value,
      userId: this.serviceForm.get('userId')!.value,
      serviceId: this.serviceForm.get('serviceId')!.value,
      vacancyId: this.selectedVacancyId ?? undefined,
      startTime: new Date(startVal).toISOString(),
      endTime: new Date(endVal).toISOString(),
      notes: this.notes || undefined,
    }).subscribe({
      next: () => {
        this.snackBar.open('Agendamento criado com sucesso! Cobrança enviada por email.', 'OK', {
          duration: 5000, panelClass: ['snackbar-success']
        });
        this.dialogRef.close(true);
      },
      error: () => { this.saving = false; },
    });
  }
}
