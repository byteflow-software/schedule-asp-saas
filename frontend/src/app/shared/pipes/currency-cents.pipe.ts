import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'currencyCents', standalone: true })
export class CurrencyCentsPipe implements PipeTransform {
  transform(valueInCents: number | null | undefined): string {
    if (valueInCents == null) return 'R$ 0,00';
    const reais = valueInCents / 100;
    return reais.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }
}
