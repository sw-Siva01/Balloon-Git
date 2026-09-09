module.exports = {
  apps: [
    {
      name:         "colyseus",
      script:       "dist/index.js",
      instances:    1,
      exec_mode:    "fork",
      watch:        false,
      max_memory_restart: "400M",
      env_production: { NODE_ENV: "production" },
    },
    {
      name:         "api",
      script:       "dist/api/index.js",
      instances:    1,
      exec_mode:    "fork",
      watch:        false,
      max_memory_restart: "200M",
      env_production: { NODE_ENV: "production" },
    },
    {
      name:         "mock-operator",
      script:       "/app/mockop/dist/index.js",
      cwd:          "/app/mockop",
      instances:    1,
      exec_mode:    "fork",
      watch:        false,
      max_memory_restart: "100M",
      env_production: { NODE_ENV: "production", PORT: "4000" },
    },
  ],
};
