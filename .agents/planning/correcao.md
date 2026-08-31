Sprint de Correção CQRS / Clean Architecture - Planejamento
1. Diagnóstico (Confirmado)
O problema: O projeto usa MediatR para CQRS (todos commands/queries usam IRequest/IRequestHandler), mas há violações em 2 níveis:
Camada API (6 pontos críticos)
- PortfolioController.cs - ResolvePortfolioIdAsync acessa _context.Portfolios direto (sem MediatR)
- SettingsController.cs - leitura + escrita + SaveChangesAsync direto no controller
- HydrologicalSimulationJob.cs - leitura direta de PrecipitationScenarios
- 3 Consumers (EnaCalculatedEventConsumer.cs, OperationPublishedEventConsumer.cs, ProspectModelRunnerConsumer.cs) - gravam/consultam no banco direto
Camada Application (~40 handlers/behaviors/serviços)
- ~30 handlers usam IEtrmDbContext + Microsoft.EntityFrameworkCore em vez de repositórios
- AuditLoggingBehavior.cs - persiste AuditLog no banco direto (como behavior transversal, mas ainda usa EF Core)
- IEtrmDbContext expõe DbSet<T> na Application - Dependency Inversion com acoplamento ao ORM
- Só existe 1 repositório (IContractRepository) para dezenas de agregados
2. Sprint Plan - Escopo Completo (API + Application)
Objetivo: Restaurar a arquitetura CQRS/Clean Architecture movendo todo acesso a banco para o padrão correto
Entregas:
Fase 1 - API (6 arquivos) - Correção rápida, alta visibilidade
1. PortfolioController.cs - Remover _context e ResolvePortfolioIdAsync direto. Criar Command ResolvePortfolioIdCommand + Handler que usa repositório (IContractRepository adaptado) ou query via MediatR. O controller apenas dispara o comando/query.
2. SettingsController.cs - Transformar GET/UPDATE em Commands/Queries via MediatR. Criar GetSettingsQuery e UpdateSettingsCommand. Controller apenas dispara.
3. HydrologicalSimulationJob.cs - Remover acesso direto a _context.PrecipitationScenarios. Criar Query GetLatestPrecipitationScenarioQuery + Handler. Job apenas dispara a query e o command RunHydrologicalSimulationCommand.
4. EnaCalculatedEventConsumer.cs - Remover acesso direto ao banco. Event deve publicar apenas eventos de domínio (ex.: HydrologicalResultGeneratedIntegrationEvent) e deixar outro processo/consumer lidar com persistência ou o sistema existente já tem outro fluxo.
5. OperationPublishedEventConsumer.cs - Mesmo padrão: publicar eventos, não acessar DB direto.
6. ProspectModelRunnerConsumer.cs - Mesmo padrão.
Fase 2 - Application (estrutura + handlers) - Correção arquitetural
 7. Criar repositórios por agregado (estender IContractRepository modelo):
- ICompanyRepository, IOperationRepository, IPortfolioRepository, IStudyRepository, IStrategyRepository, IFinanceRepository, etc.
- Implementações em Infrastructure/Repositories/ seguindo padrão ContractRepository.
 8. Refatorar handlers para usar repositórios em vez de IEtrmDbContext:
- Mover todo .Include(), .AsNoTracking(), queries do handler para o repositório.
- Handler foca apenas na regra de negócio, não em EF Core.
 9. Refatorar AuditLoggingBehavior para não depender de IEtrmDbContext:
- Receber IAuditLogRepository (novo repositório) via construtor.
- Ou receber IUnitOfWork que expõe apenas SaveChanges() sem DbSet expostos.
- Se mantiver behavior, garantir que ele use o repositório criado na injecão em vez de IEtrmDbContext.
10. Refatorar OpportunityEngineService.cs e outros serviços que injetam IEtrmDbContext para usar repositórios ou services domain-specific.
11. Remover IEtrmDbContext da Application (ou torná-la pura de domínio sem DbSet):
- Se possível, renomear/transformar a interface para não expor DbSet<T>.
- Se mante-la, garantir que nada na Application a injeje para queries/escrita — só para comportamentos específicos se precisar.
12. Atualizar NativeInjectorConfig.cs:
- Registrar todos os novos repositórios (AddScoped<ICompanyRepository, CompanyRepository>, etc.).
- Se remover IEtrmDbContext da Application, ajustar o registro do EtrmDbContext (já está como AddDbContext<EtrmDbContext>).
Dependências entre tarefas:
1. Tarefas 1-6 (API) podem começar imediatamente e compilam independentemente.
2. Tarefas 7-12 (Application) dependem de ter definido os novos repositórios (tarefa 7) antes de refatorar os handlers (8+).
3. A tarefa 9 (AuditLoggingBehavior) é crítica porque quebra o pipeline atual se mudar a assinatura — precisa ser feita com cuidado para não quebrar todos os handlers que dependem do behavior.
Critério de aceite da sprint:
- dotnet build com 0 erros (baseline já passa)
- Nenhum controller/job/consumer da API acessa _context/IEtrmDbContext diretamente — todos passam pelo MediatR
- Pelo menos 70% dos handlers da Application usam repositórios em vez de IEtrmDbContext direto
- AuditLoggingBehavior persiste audit log via novo repositório (ou mantém compatibilidade se for menor esforço)
- Padrão consistente: todos os comandos que escrevem no banco usam repositório; todas as queries usam repositório ou projection via MediatR
Risco médio/alto: A refatoração da Application (tarefas 7-12) toca em ~40 arquivos e muda a estrutura de injeção de dependência. O risco está em quebrar a cadeia de comandos/queries existentes. Recomendo:
- Começar pela Fase 1 (API) para ganhar confiança rapidamente.
- Parar para revisão após Fase 1 antes de prosseguir para Application.
- Manter IEtrmDbContext funcional durante a transição (não removê-la completamente imediatamente) para não quebrar builds.
Próximos passos:
O usuário pode validar este plano e dizer se quer que eu:
1. Comece a implementar (embora eu não possa editar em plan mode — o usuário teria que me dar permissão ou executar as mudanças).
2. Apenas gerar a lista de tasks/arquivos detalhada para ele executar.
3. Ajustar o escopo (ex.: fazer somente a Fase 1 agora).
Estou pronto para seguir com a implementação se autorizado, ou gerar o documento detalhado de tasks