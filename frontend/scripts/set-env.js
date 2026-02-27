const fs = require('fs');
const path = require('path');

const apiUrl = process.env.API_URL || '';

const content = `export const environment = {
  production: true,
  apiUrl: '${apiUrl}',
};
`;

const filePath = path.join(__dirname, '..', 'src', 'environments', 'environment.prod.ts');
fs.writeFileSync(filePath, content, 'utf-8');

console.log(`environment.prod.ts generated with apiUrl: '${apiUrl || '(empty)'}'`);
