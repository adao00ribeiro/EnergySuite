(function (window) {
  window.__env = window.__env || {};
  window.__env.KEYCLOAK_URL = window.__env.KEYCLOAK_URL || 'http://keycloak.energysuite.local';
  window.__env.KEYCLOAK_REALM = window.__env.KEYCLOAK_REALM || 'EnergySuite';
  window.__env.KEYCLOAK_CLIENT_ID = window.__env.KEYCLOAK_CLIENT_ID || 'energysuite-frontend';
  window.__env.API_GATEWAY_URL = window.__env.API_GATEWAY_URL || 'http://api.energysuite.local';
  window.__env.ENVIRONMENT = window.__env.ENVIRONMENT || 'development';
})(this);
