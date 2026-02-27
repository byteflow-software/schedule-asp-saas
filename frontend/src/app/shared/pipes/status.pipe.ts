import { Pipe, PipeTransform } from '@angular/core';

const STATUS_MAP: Record<string, string> = {
  'PendingPayment': 'Pendente Pagamento',
  'Confirmed': 'Confirmado',
  'Completed': 'Concluído',
  'Cancelled': 'Cancelado',
  'NoShow': 'Não Compareceu',
  'Pending': 'Pendente',
  'Paid': 'Pago',
  'Refunded': 'Estornado',
  'Charge': 'Cobrança',
  'Refund': 'Estorno',
};

@Pipe({ name: 'statusTranslate', standalone: true })
export class StatusTranslatePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '';
    return STATUS_MAP[value] ?? value;
  }
}
