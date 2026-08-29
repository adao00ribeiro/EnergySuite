export const environment = {
  production: true,
  apiUrl: (window as any).env?.apiUrl || 'http://localhost:8080/api/v1',
  riskApiUrl: (window as any).env?.riskApiUrl || 'http://localhost:8000/api/v1'
};
