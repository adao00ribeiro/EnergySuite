export const environment = {
  production: true,
  apiUrl: (window as any).env?.apiUrl || 'http://localhost:8080/api/v1',
  prospectHubUrl: (window as any).env?.prospectHubUrl || 'http://localhost:8080/hubs/prospect'
};
