import { DateFormatPipe } from './date-format.pipe';

describe('DateFormatPipe', () => {
  let pipe: DateFormatPipe;

  beforeEach(() => {
    pipe = new DateFormatPipe();
  });

  it('should return empty string for null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('should return empty string for undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });

  it('should format date with short format (default)', () => {
    const result = pipe.transform('2025-06-15T10:30:00');
    // pt-BR short format: dd/mm/yyyy
    expect(result).toContain('15');
    expect(result).toContain('06');
    expect(result).toContain('2025');
  });

  it('should format date with full format including time', () => {
    const result = pipe.transform('2025-06-15T10:30:00', 'full');
    // pt-BR full format: dd/mm/yyyy hh:mm
    expect(result).toContain('15');
    expect(result).toContain('06');
    expect(result).toContain('2025');
    expect(result).toContain('10');
    expect(result).toContain('30');
  });

  it('should format date with time format', () => {
    const result = pipe.transform('2025-06-15T14:45:00', 'time');
    expect(result).toContain('14');
    expect(result).toContain('45');
  });
});
