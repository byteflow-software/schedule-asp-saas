import { StatusTranslatePipe } from './status.pipe';

describe('StatusTranslatePipe', () => {
  let pipe: StatusTranslatePipe;

  beforeEach(() => {
    pipe = new StatusTranslatePipe();
  });

  it('should translate PendingPayment to Pendente Pagamento', () => {
    expect(pipe.transform('PendingPayment')).toBe('Pendente Pagamento');
  });

  it('should translate Confirmed to Confirmado', () => {
    expect(pipe.transform('Confirmed')).toBe('Confirmado');
  });

  it('should translate Completed to Concluído', () => {
    expect(pipe.transform('Completed')).toBe('Concluído');
  });

  it('should translate Cancelled to Cancelado', () => {
    expect(pipe.transform('Cancelled')).toBe('Cancelado');
  });

  it('should return original value for unknown status', () => {
    expect(pipe.transform('SomeUnknownStatus')).toBe('SomeUnknownStatus');
  });

  it('should return empty string for null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('should return empty string for undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });
});
