const fs = require('fs');

const files = [
  'src/app/features/settings/tenant-info/tenant-info.component.ts',
  'src/app/features/customers/customer-list/customer-list.component.ts',
  'src/app/features/appointments/appointment-list/appointment-list.component.ts',
  'src/app/features/appointments/appointment-calendar/appointment-calendar.component.ts',
  'src/app/features/appointments/appointment-detail/appointment-detail.component.ts',
  'src/app/features/vacancies/vacancy-list/vacancy-list.component.ts',
  'src/app/features/users/user-list/user-list.component.ts',
  'src/app/features/services/service-list/service-list.component.ts',
  'src/app/features/finance/finance-dashboard/finance-dashboard.component.ts',
  'src/app/features/auth/login/login.component.ts',
  'src/app/features/auth/register/register.component.ts',
  'src/app/features/dashboard/dashboard.component.ts',
];

files.forEach(f => {
  let code = fs.readFileSync(f, 'utf-8');

  // 1. Add 'signal' to the @angular/core import if not present
  if (code.indexOf('signal') === -1) {
    code = code.replace(
      /(import\s*\{[^}]*)\}\s*from\s*'@angular\/core'/,
      '$1, signal } from \'@angular/core\''
    );
  }

  // 2. Property declaration: loading = true; or loading = false;
  code = code.replace(/^(\s+)loading = (true|false);/gm, '$1loading = signal($2);');

  // 3. this.loading = true; -> this.loading.set(true);
  code = code.replace(/this\.loading = true;/g, 'this.loading.set(true);');

  // 4. this.loading = false; -> this.loading.set(false);
  code = code.replace(/this\.loading = false;/g, 'this.loading.set(false);');

  // 5. Template: @if (loading) -> @if (loading())
  code = code.replace(/@if \(loading\)/g, '@if (loading())');

  // 6. Template: || loading" -> || loading()"
  code = code.replace(/\|\| loading"/g, '|| loading()"');
  code = code.replace(/\|\| loading'/g, "|| loading()'");

  fs.writeFileSync(f, code, 'utf-8');
  console.log('Updated:', f);
});

console.log('Done!');
