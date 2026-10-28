# 🎨 Estúdio de Tatuagem API

API REST desenvolvida em ASP.NET Core (.NET 10) para gerenciamento de um Estúdio de Tatuagem, com controle de tatuadores, agendamento de sessões e autenticação.

## 📌 Links do Projeto
- **API no Ar (Render):** https://estudio-tatuagem-api.onrender.com
- **Documentação (Swagger):** https://estudio-tatuagem-api.onrender.com/swagger
- **Health Check:** https://estudio-tatuagem-api.onrender.com/health
- **Versão:** https://estudio-tatuagem-api.onrender.com/api/versao

## ⚙️ Tecnologias Utilizadas
- .NET 10 (C# 14) / ASP.NET Core Web API
- Entity Framework Core 10 com PostgreSQL (Neon)
- Autenticação JWT com encriptação BCrypt
- Swagger / Swashbuckle 10
- Testes Unitários com xUnit e SQLite em memória
- Docker & Render (Deploy Contínuo)
- GitHub Actions (CI)

## 💼 Regras de Negócio e Entidades
1. **Usuários:** Autenticação via e-mail e senha com papéis de `admin` e `cliente`.
2. **Tatuador (Entidade Principal):** Cadastro completo de tatuadores com estilo principal e preço mínimo. Listagem pública paginada e com busca.
3. **Sessão (Segunda Entidade - 1:N):** Agendamento de sessões vinculado ao cliente e ao tatuador.
4. **Regra de Conflito (409 Conflict):** O sistema impede o agendamento de duas sessões para o mesmo tatuador no mesmo horário, retornando o status HTTP `409 Conflict`.

## 🧪 Como rodar os testes
No terminal do projeto, execute:
```bash
dotnet test