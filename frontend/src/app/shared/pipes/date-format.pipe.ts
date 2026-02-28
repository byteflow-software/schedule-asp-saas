import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'dateFormat', standalone: true })
export class DateFormatPipe implements PipeTransform {
  transform(value: string | null | undefined, format: 'short' | 'time' | 'full' = 'short'): string {
    if (!value) return '';
    const date = new Date(value);
    const options: Intl.DateTimeFormatOptions =
      format === 'time'
        ? { hour: '2-digit', minute: '2-digit' }
        : format === 'full'
        ? { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }
        : { day: '2-digit', month: '2-digit', year: 'numeric' };
    return date.toLocaleDateString('pt-BR', options);
  }
}
