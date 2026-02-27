import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { FormsModule } from '@angular/forms';
import { VacancyService } from '../../../core/services/vacancy.service';
import { UserService } from '../../../core/services/user.service';
import { ServiceService } from '../../../core/services/service.service';
import { UserDto } from '../../../core/models/user.model';
import { ServiceDto } from '../../../core/models/service.model';

interface GeneratedSlot {
  date: Date;
  startTime: string;
  endTime: string;
  dateLabel: string;
  timeLabel: string;
}

@Component({
  selector: 'app-vacancy-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatDatepickerModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDividerModule,
  ],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>Abrir Vagas</h2>
      <p class="dialog-subtitle">Gere horários disponíveis para agendamento</p>
    </div>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-fields">
        <!-- Profissional e Serviço -->
        <div class="form-row">
          <mat-form-field class="full-width" appearance="outline">
            <mat-label>Profissional</mat-label>
            <mat-icon matPrefix>badge</mat-icon>
            <mat-select formControlName="userId">
              @for (u of users; track u.id) {
                <mat-option [value]="u.id">{{ u.fullName }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field class="full-width" appearance="outline">
            <mat-label>Serviço</mat-label>
            <mat-icon matPrefix>medical_services</mat-icon>
            <mat-select formControlName="serviceId" (selectionChange)="onServiceChange()">
              @for (s of services; track s.id) {
                <mat-option [value]="s.id">{{ s.name }} ({{ s.durationMinutes }}min)</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </div>

        <!-- Período de datas -->
        <div class="section">
          <div class="section-title">
            <mat-icon>date_range</mat-icon>
            <span>Período</span>
          </div>
          <div class="form-row">
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Data Início</mat-label>
              <input matInput [matDatepicker]="pickerStart" formControlName="dateStart" />
              <mat-datepicker-toggle matIconSuffix [for]="pickerStart"></mat-datepicker-toggle>
              <mat-datepicker #pickerStart></mat-datepicker>
            </mat-form-field>
            <mat-form-field class="full-width" appearance="outline">
              <mat-label>Data Fim</mat-label>
              <input matInput [matDatepicker]="pickerEnd" formControlName="dateEnd" />
              <mat-datepicker-toggle matIconSuffix [for]="pickerEnd"></mat-datepicker-toggle>
              <mat-datepicker #pickerEnd></mat-datepicker>
            </mat-form-field>
          </div>
          <div class="weekdays">
            <span class="weekdays-label">Dias da semana:</span>
            <div class="weekday-chips">
              @for (day of weekDays; track day.value) {
                <button type="button" class="weekday-chip"
                  [class.selected]="day.selected"
                  (click)="day.selected = !day.selected">
                  {{ day.short }}
                </button>
              }
            </div>
          </div>
        </div>

        <!-- Horários -->
        <div class="section">
          <div class="section-title">
            <mat-icon>schedule</mat-icon>
            <span>Horários</span>
          </div>
          <div class="form-row-3">
            <mat-form-field appearance="outline">
              <mat-label>Início</mat-label>
              <input matInput type="time" formControlName="rangeStart" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Fim</mat-label>
              <input matInput type="time" formControlName="rangeEnd" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Intervalo (min)</mat-label>
              <input matInput type="number" formControlName="interval" min="0" />
            </mat-form-field>
          </div>

          <button mat-flat-button color="primary" type="button" class="gen-btn"
            [disabled]="!canGenerate()" (click)="generateSlots()">
            <mat-icon>auto_fix_high</mat-icon> Gerar Horários
          </button>
        </div>

        <!-- Preview dos slots -->
        @if (slots.length > 0) {
          <mat-divider></mat-divider>
          <div class="preview-section">
            <div class="preview-header">
              <span class="preview-title">
                <mat-icon>event_available</mat-icon>
                {{ slots.length }} vaga{{ slots.length !== 1 ? 's' : '' }} em {{ uniqueDatesCount }} dia{{ uniqueDatesCount !== 1 ? 's' : '' }}
              </span>
              <button mat-button color="warn" type="button" (click)="slots = []">
                <mat-icon>delete_sweep</mat-icon> Limpar
              </button>
            </div>
            <div class="date-groups">
              @for (group of groupedSlots; track group.dateLabel) {
                <div class="date-group">
                  <div class="date-group-header">
                    <mat-icon>calendar_today</mat-icon>
                    <span>{{ group.dateLabel }}</span>
                    <span class="slot-count">{{ group.slots.length }} vaga{{ group.slots.length !== 1 ? 's' : '' }}</span>
                  </div>
                  <div class="time-chips">
                    @for (slot of group.slots; track slot.timeLabel; let i = $index) {
                      <div class="time-chip">
                        <span>{{ slot.timeLabel }}</span>
                        <button type="button" class="remove-btn" (click)="removeSlot(slot)">
                          <mat-icon>close</mat-icon>
                        </button>
                      </div>
                    }
                  </div>
                </div>
              }
            </div>
          </div>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancelar</button>
      <button mat-flat-button color="primary" [disabled]="!canSave() || saving" (click)="save()">
        @if (saving) {
          Salvando...
        } @else {
          Criar {{ slots.length }} Vaga{{ slots.length !== 1 ? 's' : '' }}
        }
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-header { padding: 24px 24px 0; }
    h2 { margin: 0; font-size: 20px; font-weight: 700; color: #111827; }
    .dialog-subtitle { margin: 4px 0 0; font-size: 14px; color: #6B7280; }
    .form-fields { display: flex; flex-direction: column; gap: 8px; padding-top: 8px; }
    .form-row { display: flex; gap: 12px; > * { flex: 1; } }
    .form-row-3 { display: flex; gap: 12px; > * { flex: 1; } }

    .section {
      background: #F9FAFB; border-radius: 12px; padding: 16px;
      border: 1px solid #E5E7EB;
    }
    .section-title {
      display: flex; align-items: center; gap: 8px;
      font-size: 14px; font-weight: 600; color: #374151;
      margin-bottom: 12px;
      mat-icon { font-size: 18px; width: 18px; height: 18px; color: var(--primary); }
    }

    .weekdays { margin-top: 4px; }
    .weekdays-label { font-size: 13px; color: #6B7280; margin-bottom: 8px; display: block; }
    .weekday-chips { display: flex; gap: 6px; flex-wrap: wrap; }
    .weekday-chip {
      width: 40px; height: 36px; border-radius: 8px;
      border: 1.5px solid #D1D5DB; background: white;
      font-size: 13px; font-weight: 600; color: #6B7280;
      cursor: pointer; transition: all 0.15s;
      display: flex; align-items: center; justify-content: center;
      &:hover { border-color: var(--primary); color: var(--primary); }
      &.selected {
        background: var(--primary); border-color: var(--primary);
        color: white;
      }
    }

    .gen-btn {
      width: 100%; margin-top: 8px;
      mat-icon { margin-right: 4px; }
    }

    .preview-section { }
    .preview-header {
      display: flex; justify-content: space-between; align-items: center;
      margin-bottom: 12px;
    }
    .preview-title {
      display: flex; align-items: center; gap: 8px;
      font-size: 14px; font-weight: 600; color: #374151;
      mat-icon { font-size: 18px; width: 18px; height: 18px; color: var(--success); }
    }

    .date-groups { display: flex; flex-direction: column; gap: 12px; }
    .date-group {
      background: white; border-radius: 10px; padding: 12px;
      border: 1px solid #E5E7EB;
    }
    .date-group-header {
      display: flex; align-items: center; gap: 8px;
      font-size: 13px; font-weight: 600; color: #374151;
      margin-bottom: 8px;
      mat-icon { font-size: 16px; width: 16px; height: 16px; color: var(--primary); }
    }
    .slot-count {
      margin-left: auto; font-size: 12px; font-weight: 500;
      color: #6B7280; background: #F3F4F6; padding: 2px 8px;
      border-radius: 10px;
    }
    .time-chips { display: flex; flex-wrap: wrap; gap: 6px; }
    .time-chip {
      display: flex; align-items: center; gap: 4px;
      padding: 4px 10px; border-radius: 8px; font-size: 12px;
      font-weight: 500; background: #EEF2FF; color: #4338CA;
      border: 1px solid #C7D2FE;
    }
    .remove-btn {
      background: none; border: none; cursor: pointer; padding: 0;
      display: flex; align-items: center; color: #9CA3AF;
      mat-icon { font-size: 14px; width: 14px; height: 14px; }
      &:hover { color: var(--danger); }
    }

    mat-divider { margin: 8px 0; }
  `],
})
export class VacancyFormComponent implements OnInit {
  dialogRef = inject(MatDialogRef<VacancyFormComponent>);
  private fb = inject(FormBuilder);
  private vacancyService = inject(VacancyService);
  private userService = inject(UserService);
  private serviceService = inject(ServiceService);
  private snackBar = inject(MatSnackBar);

  users: UserDto[] = [];
  services: ServiceDto[] = [];
  slots: GeneratedSlot[] = [];
  saving = false;

  weekDays = [
    { short: 'Seg', value: 1, selected: true },
    { short: 'Ter', value: 2, selected: true },
    { short: 'Qua', value: 3, selected: true },
    { short: 'Qui', value: 4, selected: true },
    { short: 'Sex', value: 5, selected: true },
    { short: 'Sáb', value: 6, selected: false },
    { short: 'Dom', value: 0, selected: false },
  ];

  form = this.fb.nonNullable.group({
    userId: ['', Validators.required],
    serviceId: ['', Validators.required],
    dateStart: [null as Date | null, Validators.required],
    dateEnd: [null as Date | null, Validators.required],
    rangeStart: ['08:00'],
    rangeEnd: ['18:00'],
    interval: [0],
  });

  get uniqueDatesCount(): number {
    const dates = new Set(this.slots.map(s => s.dateLabel));
    return dates.size;
  }

  get groupedSlots(): { dateLabel: string; slots: GeneratedSlot[] }[] {
    const map = new Map<string, GeneratedSlot[]>();
    for (const slot of this.slots) {
      const existing = map.get(slot.dateLabel) || [];
      existing.push(slot);
      map.set(slot.dateLabel, existing);
    }
    return Array.from(map.entries()).map(([dateLabel, slots]) => ({ dateLabel, slots }));
  }

  ngOnInit(): void {
    // Default: today to next week
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const nextWeek = new Date(today);
    nextWeek.setDate(nextWeek.getDate() + 6);
    this.form.patchValue({ dateStart: today, dateEnd: nextWeek });

    this.userService.getAll().subscribe({
      next: (users) => { this.users = users.filter(u => u.isActive); },
    });
    this.serviceService.getAll().subscribe({
      next: (services) => { this.services = services.filter(s => s.isActive); },
    });
  }

  onServiceChange(): void {
    const serviceId = this.form.getRawValue().serviceId;
    const service = this.services.find(s => s.id === serviceId);
    if (service) {
      this.slots = [];
    }
  }

  canGenerate(): boolean {
    const v = this.form.getRawValue();
    return !!v.serviceId && !!v.dateStart && !!v.dateEnd && !!v.rangeStart && !!v.rangeEnd;
  }

  canSave(): boolean {
    return this.form.get('userId')!.valid && this.form.get('serviceId')!.valid && this.slots.length > 0;
  }

  generateSlots(): void {
    const v = this.form.getRawValue();
    const service = this.services.find(s => s.id === v.serviceId);
    if (!service || !v.dateStart || !v.dateEnd) return;

    const duration = service.durationMinutes;
    const interval = v.interval || 0;
    const startMinutes = this.timeToMinutes(v.rangeStart);
    const endMinutes = this.timeToMinutes(v.rangeEnd);
    if (startMinutes === null || endMinutes === null || duration <= 0) return;

    const selectedDays = this.weekDays.filter(d => d.selected).map(d => d.value);
    const dates = this.getDateRange(v.dateStart, v.dateEnd, selectedDays);

    for (const date of dates) {
      let current = startMinutes;
      while (current + duration <= endMinutes) {
        const slot = this.buildSlot(date, current, current + duration);
        if (!this.isDuplicate(slot)) {
          this.slots.push(slot);
        }
        current += duration + interval;
      }
    }

    this.slots.sort((a, b) => a.startTime.localeCompare(b.startTime));
  }

  removeSlot(slot: GeneratedSlot): void {
    const idx = this.slots.indexOf(slot);
    if (idx >= 0) this.slots.splice(idx, 1);
  }

  save(): void {
    if (!this.canSave()) return;
    this.saving = true;

    const v = this.form.getRawValue();
    this.vacancyService.bulkCreate({
      userId: v.userId,
      serviceId: v.serviceId,
      slots: this.slots.map(s => ({ startTime: s.startTime, endTime: s.endTime })),
    }).subscribe({
      next: (created) => {
        this.snackBar.open(
          `${created.length} vaga${created.length !== 1 ? 's' : ''} criada${created.length !== 1 ? 's' : ''} com sucesso!`,
          'OK',
          { duration: 3000, panelClass: ['snackbar-success'] },
        );
        this.dialogRef.close(true);
      },
      error: () => {
        this.saving = false;
        this.snackBar.open('Erro ao criar vagas.', 'OK', { duration: 3000, panelClass: ['snackbar-error'] });
      },
    });
  }

  private getDateRange(start: Date, end: Date, allowedDays: number[]): Date[] {
    const dates: Date[] = [];
    const current = new Date(start);
    current.setHours(0, 0, 0, 0);
    const endDate = new Date(end);
    endDate.setHours(23, 59, 59, 999);

    while (current <= endDate) {
      if (allowedDays.includes(current.getDay())) {
        dates.push(new Date(current));
      }
      current.setDate(current.getDate() + 1);
    }
    return dates;
  }

  private buildSlot(date: Date, startMinutes: number, endMinutes: number): GeneratedSlot {
    const d = new Date(date);
    const start = new Date(d);
    start.setHours(Math.floor(startMinutes / 60), startMinutes % 60, 0, 0);
    const end = new Date(d);
    end.setHours(Math.floor(endMinutes / 60), endMinutes % 60, 0, 0);

    const fmt = (m: number) => {
      const h = Math.floor(m / 60).toString().padStart(2, '0');
      const min = (m % 60).toString().padStart(2, '0');
      return `${h}:${min}`;
    };

    return {
      date: d,
      startTime: start.toISOString(),
      endTime: end.toISOString(),
      dateLabel: d.toLocaleDateString('pt-BR', { weekday: 'short', day: '2-digit', month: '2-digit' }),
      timeLabel: `${fmt(startMinutes)} - ${fmt(endMinutes)}`,
    };
  }

  private timeToMinutes(time: string): number | null {
    if (!time) return null;
    const [h, m] = time.split(':').map(Number);
    return h * 60 + m;
  }

  private isDuplicate(slot: GeneratedSlot): boolean {
    return this.slots.some(s => s.startTime === slot.startTime && s.endTime === slot.endTime);
  }
}
