import { CurrencyCentsPipe } from './currency-cents.pipe';

describe('CurrencyCentsPipe', () => {
  const pipe = new CurrencyCentsPipe();

  it('should return "R$ 0,00" for null', () => {
    expect(pipe.transform(null)).toBe('R$ 0,00');
  });

  it('should return "R$ 0,00" for undefined', () => {
    expect(pipe.transform(undefined)).toBe('R$ 0,00');
  });

  it('should format 0 as zero currency', () => {
    const result = pipe.transform(0);
    expect(result).toContain('0,00');
  });

  it('should format 5000 cents as R$50', () => {
    const result = pipe.transform(5000);
    expect(result).toContain('50,00');
  });

  it('should format 12345 cents correctly', () => {
    const result = pipe.transform(12345);
    expect(result).toContain('123,45');
  });
});
