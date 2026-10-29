const API_URL = "https://estudio-tatuagem-api.onrender.com";

let state = {
    token: localStorage.getItem('tattoo_token') || '',
    user: JSON.parse(localStorage.getItem('tattoo_user') || 'null'),
    tatuadores: [],
    paginaAtual: 1,
    totalPaginas: 1
};

// Imagens fakes realistas para cada artista
const mockAvatars = [
    "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&q=80&w=400",
    "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&q=80&w=400",
    "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&q=80&w=400",
    "https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&q=80&w=400",
    "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?auto=format&fit=crop&q=80&w=400"
];

window.onload = () => {
    initAuthUI();
    carregarTatuadores();
    checkApiHealth();
};

// --- SISTEMA DE AUTENTICAÇÃO ---
function initAuthUI() {
    const loggedIn = !!state.token;

    // Elements
    const navUserLogged = document.getElementById('navUserLogged');
    const btnOpenAuthNav = document.getElementById('btnOpenAuthNav');
    const mobileUserLogged = document.getElementById('mobileUserLogged');
    const mobileBtnAuth = document.getElementById('mobileBtnAuth');
    const navLinkMeusAgendamentos = document.getElementById('navLinkMeusAgendamentos');
    const mobileLinkMeusAgendamentos = document.getElementById('mobileLinkMeusAgendamentos');

    const cardBloqueado = document.getElementById('cardAgendamentoBloqueado');
    const formAgendamento = document.getElementById('formAgendamentoContainer');

    if (loggedIn && state.user) {
        // Desktop Nav
        navUserLogged.classList.remove('hidden');
        navUserLogged.classList.add('flex');
        btnOpenAuthNav.classList.add('hidden');

        document.getElementById('userName').innerText = state.user.nome || 'Cliente';
        document.getElementById('userRole').innerText = state.user.papel || 'cliente';
        document.getElementById('userAvatar').innerText = (state.user.nome || 'U').charAt(0).toUpperCase();

        // Mobile Nav
        mobileUserLogged.classList.remove('hidden');
        mobileUserLogged.classList.add('flex');
        mobileBtnAuth.classList.add('hidden');
        document.getElementById('mobileUserName').innerText = state.user.nome || 'Cliente';

        // Links
        navLinkMeusAgendamentos.classList.remove('hidden');
        mobileLinkMeusAgendamentos.classList.remove('hidden');

        // Agendamento Form Visible
        cardBloqueado.classList.add('hidden');
        formAgendamento.classList.remove('hidden');

        carregarMinhasSessoes();
    } else {
        navUserLogged.classList.add('hidden');
        btnOpenAuthNav.classList.remove('hidden');
        mobileUserLogged.classList.add('hidden');
        mobileBtnAuth.classList.remove('hidden');

        navLinkMeusAgendamentos.classList.add('hidden');
        mobileLinkMeusAgendamentos.classList.add('hidden');

        cardBloqueado.classList.remove('hidden');
        formAgendamento.classList.add('hidden');
    }
}

async function handleLogin(e) {
    e.preventDefault();
    const email = document.getElementById('loginEmail').value;
    const senha = document.getElementById('loginSenha').value;
    const feedback = document.getElementById('loginFeedback');
    const btn = document.getElementById('btnSubmitLogin');

    feedback.classList.add('hidden');
    btn.disabled = true;
    btn.innerHTML = `<i class="fa-solid fa-spinner fa-spin mr-2"></i> Entrando...`;

    try {
        const res = await fetch(`${API_URL}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, senha })
        });

        const data = await res.json();

        if (res.ok) {
            state.token = data.token;
            state.user = { nome: data.nome, email: data.email, papel: data.papel };
            localStorage.setItem('tattoo_token', data.token);
            localStorage.setItem('tattoo_user', JSON.stringify(state.user));

            initAuthUI();
            closeAuthModal();
            showNotification('Sucesso', 'Login realizado com sucesso!', 'success');
        } else {
            feedback.className = "p-3 rounded-lg text-xs bg-red-500/10 border border-red-500/30 text-red-400";
            feedback.innerText = data.detail || "Falha ao realizar login. Verifique suas credenciais.";
            feedback.classList.remove('hidden');
        }
    } catch (err) {
        feedback.className = "p-3 rounded-lg text-xs bg-red-500/10 border border-red-500/30 text-red-400";
        feedback.innerText = "Erro ao se conectar com o servidor da API.";
        feedback.classList.remove('hidden');
    } finally {
        btn.disabled = false;
        btn.innerText = "Entrar";
    }
}

async function handleRegistro(e) {
    e.preventDefault();
    const nome = document.getElementById('registroNome').value;
    const email = document.getElementById('registroEmail').value;
    const senha = document.getElementById('registroSenha').value;
    const feedback = document.getElementById('registroFeedback');
    const btn = document.getElementById('btnSubmitRegistro');

    feedback.classList.add('hidden');
    btn.disabled = true;
    btn.innerHTML = `<i class="fa-solid fa-spinner fa-spin mr-2"></i> Criando conta...`;

    try {
        const res = await fetch(`${API_URL}/api/auth/registro`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ nome, email, senha })
        });

        const data = await res.json();

        if (res.status === 201) {
            // Tenta fazer o login automático após criar a conta
            document.getElementById('loginEmail').value = email;
            document.getElementById('loginSenha').value = senha;
            await handleLogin(e);
        } else {
            feedback.className = "p-3 rounded-lg text-xs bg-red-500/10 border border-red-500/30 text-red-400";
            feedback.innerText = data.detail || "Não foi possível cadastrar a conta.";
            feedback.classList.remove('hidden');
        }
    } catch (err) {
        feedback.className = "p-3 rounded-lg text-xs bg-red-500/10 border border-red-500/30 text-red-400";
        feedback.innerText = "Erro na requisição ao servidor.";
        feedback.classList.remove('hidden');
    } finally {
        btn.disabled = false;
        btn.innerText = "Cadastrar e Entrar";
    }
}

function logout() {
    state.token = '';
    state.user = null;
    localStorage.removeItem('tattoo_token');
    localStorage.removeItem('tattoo_user');
    initAuthUI();
    showNotification('Sessão Encerrada', 'Você saiu da sua conta.', 'info');
}// Função de Logout reativa
function fazerLogout() {
    // 1. Limpa as credenciais/tokens salvas
    token = "";
    localStorage.removeItem("token");
    localStorage.removeItem("usuario");

    // 2. Limpa e atualiza automaticamente a área da agenda/sessões
    const containerSessoes = document.getElementById("listaSessoes");
    if (containerSessoes) {
        containerSessoes.innerHTML = "<p>Faça login para visualizar suas sessões.</p>";
    }

    // 3. Oculta ou limpa o painel de perfil se estiver aberto
    const painelPerfil = document.getElementById("painelPerfil");
    if (painelPerfil) {
        painelPerfil.style.display = "none";
    }

    // 4. Atualiza os botões do cabeçalho
    atualizarHeader();
    
    // Notifica o usuário sem precisar de recarregar a página
    exibirMensagem("authMsg", "Você saiu da conta.", "success");
}

// Função para Alterar a Senha do Usuário Logado
async function alterarSenha() {
    const msgDiv = document.getElementById("perfilMsg");
    const senhaAtual = document.getElementById("senhaAtual").value;
    const novaSenha = document.getElementById("novaSenha").value;

    if (!token) {
        exibirMensagem("perfilMsg", "Sessão expirada. Faça login novamente.", "error");
        return;
    }

    if (!senhaAtual || !novaSenha) {
        exibirMensagem("perfilMsg", "Preencha a senha atual e a nova senha!", "error");
        return;
    }

    try {
        const res = await fetch(`${API_URL}/api/usuarios/me/senha`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ senhaAtual, novaSenha })
        });

        const data = await res.json();

        if (res.ok) {
            exibirMensagem("perfilMsg", "Senha alterada com sucesso!", "success");
            document.getElementById("senhaAtual").value = "";
            document.getElementById("novaSenha").value = "";
        } else {
            exibirMensagem("perfilMsg", data.detail || "Erro ao alterar a senha.", "error");
        }
    } catch (err) {
        exibirMensagem("perfilMsg", "Erro de conexão ao alterar a senha.", "error");
    }
}

// Função auxiliar para exibir mensagens de status de forma limpa
function exibirMensagem(elementId, mensagem, tipo) {
    const el = document.getElementById(elementId);
    if (!el) return;
    el.className = `status-msg status-${tipo}`;
    el.innerText = mensagem;
    el.style.display = "block";
}

// --- CARREGAR E FILTRAR TATUADORES ---
async function carregarTatuadores(busca = '', pagina = 1) {
    const grid = document.getElementById('gridTatuadores');
    const select = document.getElementById('selectTatuador');

    try {
        const res = await fetch(`${API_URL}/api/tatuadores?busca=${encodeURIComponent(busca)}&pagina=${pagina}&tamanho=12`);
        if (!res.ok) throw new Error("Erro na busca");

        const data = await res.json();
        state.tatuadores = data.itens;
        state.paginaAtual = data.pagina;
        state.totalPaginas = data.totalPaginas;

        renderGridTatuadores(data.itens);
        renderSelectTatuadores(data.itens);
        renderPaginacao();
    } catch (err) {
        grid.innerHTML = `
                    <div class="col-span-full py-12 text-center text-gray-400 space-y-2">
                        <i class="fa-solid fa-triangle-exclamation text-3xl text-gold-400"></i>
                        <p>Erro ao carregar a lista de tatuadores da API.</p>
                    </div>
                `;
    }
}

function renderGridTatuadores(itens) {
    const grid = document.getElementById('gridTatuadores');
    if (!itens || itens.length === 0) {
        grid.innerHTML = `
                    <div class="col-span-full py-12 text-center text-gray-400">
                        Nenhum tatuador encontrado para este termo de busca.
                    </div>
                `;
        return;
    }

    grid.innerHTML = itens.map((t, idx) => {
        const img = mockAvatars[idx % mockAvatars.length];
        return `
                    <div class="bg-darkCard border border-darkBorder rounded-xl overflow-hidden hover:border-gold-500/40 transition-all group flex flex-col justify-between">
                        <div>
                            <div class="h-52 overflow-hidden relative">
                                <img src="${img}" alt="${t.nome}" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500">
                                <div class="absolute inset-0 bg-gradient-to-t from-darkCard via-transparent to-transparent"></div>
                                <span class="absolute bottom-3 left-3 bg-black/70 backdrop-blur-md px-2.5 py-1 rounded text-[11px] font-semibold text-gold-400 border border-gold-500/30">
                                    ${t.estiloPrincipal}
                                </span>
                            </div>
                            <div class="p-5 space-y-2">
                                <h3 class="font-title text-lg font-bold text-white group-hover:text-gold-400 transition-colors">${t.nome}</h3>
                                <p class="text-xs text-gray-400">Valor mínimo p/ sessão:</p>
                                <p class="text-sm font-bold text-gold-400">R$ ${t.precoMinimo.toFixed(2)}</p>
                            </div>
                        </div>
                        <div class="p-5 pt-0">
                            <button onclick='openTatuadorModal(${JSON.stringify(t).replace(/'/g, "&apos;")})' class="w-full bg-darkBg hover:bg-gold-500 hover:text-black border border-darkBorder text-gray-300 font-semibold text-xs py-2.5 rounded-lg transition-all">
                                Ver Mais Detalhes
                            </button>
                        </div>
                    </div>
                `;
    }).join('');
}

function renderSelectTatuadores(itens) {
    const select = document.getElementById('selectTatuador');
    select.innerHTML = `<option value="">Selecione um profissional...</option>` +
        itens.map(t => `<option value="${t.id}">${t.nome} (${t.estiloPrincipal})</option>`).join('');
}

function renderPaginacao() {
    const container = document.getElementById('paginationControls');
    if (state.totalPaginas <= 1) {
        container.innerHTML = '';
        return;
    }

    let html = '';
    for (let i = 1; i <= state.totalPaginas; i++) {
        const active = i === state.paginaAtual;
        html += `
                    <button onclick="mudarPagina(${i})" class="w-9 h-9 rounded-lg text-xs font-bold transition-all ${active ? 'bg-gold-500 text-black shadow-md' : 'bg-darkCard text-gray-400 border border-darkBorder hover:border-gold-500'}">
                        ${i}
                    </button>
                `;
    }
    container.innerHTML = html;
}

function mudarPagina(p) {
    const termo = document.getElementById('inputBuscaTatuador').value;
    carregarTatuadores(termo, p);
}

let searchTimeout;
function filtrarTatuadores() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        const termo = document.getElementById('inputBuscaTatuador').value;
        carregarTatuadores(termo, 1);
    }, 300);
}

// --- MANIPULAÇÃO DE AGENDAMENTO (REGRA DO 409 CONFLICT) ---
async function handleAgendarSessao(e) {
    e.preventDefault();

    const tatuadorId = parseInt(document.getElementById('selectTatuador').value);
    const dataHoraRaw = document.getElementById('inputDataHora').value;
    const descricaoTatuagem = document.getElementById('inputDescricao').value;
    const feedback = document.getElementById('agendamentoFeedback');
    const btn = document.getElementById('btnSubmitAgendamento');

    if (!tatuadorId || !dataHoraRaw || !descricaoTatuagem) {
        feedback.className = "p-4 rounded-lg text-sm bg-red-500/10 border border-red-500/30 text-red-400";
        feedback.innerText = "Por favor, preencha todos os campos do formulário.";
        feedback.classList.remove('hidden');
        return;
    }

    // Converter para UTC ISO
    const dataHoraUtc = new Date(dataHoraRaw).toISOString();

    feedback.classList.add('hidden');
    btn.disabled = true;
    btn.innerHTML = `<i class="fa-solid fa-spinner fa-spin mr-2"></i> Verificando disponibilidade...`;

    try {
        const res = await fetch(`${API_URL}/api/sessoes`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${state.token}`
            },
            body: JSON.stringify({ tatuadorId, dataHora: dataHoraUtc, descricaoTatuagem })
        });

        const data = await res.json();

        if (res.status === 201) {
            feedback.className = "p-4 rounded-lg text-sm bg-accent/10 border border-accent/40 text-accent";
            feedback.innerText = "Sessão agendada com sucesso! O seu horário foi reservado.";
            feedback.classList.remove('hidden');

            document.getElementById('formAgendamento').reset();
            carregarMinhasSessoes();
            showNotification('Sessão Agendada!', 'Seu horário foi reservado com sucesso.', 'success');
        } else if (res.status === 409) {
            // REGRA DE CONFLITO DE HORÁRIO
            feedback.className = "p-4 rounded-lg text-sm bg-red-500/10 border border-red-500/30 text-red-400 flex items-start gap-3";
            feedback.innerHTML = `
                        <i class="fa-solid fa-triangle-exclamation text-lg mt-0.5"></i>
                        <div>
                            <strong class="block font-bold">Conflito de Agendamento (Erro 409):</strong>
                            <span>${data.detail || 'O tatuador selecionado já possui uma sessão agendada para este mesmo horário. Por favor, escolha outro horário ou artista.'}</span>
                        </div>
                    `;
            feedback.classList.remove('hidden');
        } else {
            feedback.className = "p-4 rounded-lg text-sm bg-red-500/10 border border-red-500/30 text-red-400";
            feedback.innerText = data.detail || "Erro ao efetuar agendamento. Verifique os dados.";
            feedback.classList.remove('hidden');
        }
    } catch (err) {
        feedback.className = "p-4 rounded-lg text-sm bg-red-500/10 border border-red-500/30 text-red-400";
        feedback.innerText = "Erro ao se comunicar com a API de agendamentos.";
        feedback.classList.remove('hidden');
    } finally {
        btn.disabled = false;
        btn.innerHTML = `<i class="fa-solid fa-calendar-check mr-2"></i> Confirmar Agendamento`;
    }
}

// --- BUSCAR MINHAS SESSÕES ---
async function carregarMinhasSessoes() {
    if (!state.token) return;
    const grid = document.getElementById('gridMinhasSessoes');

    try {
        const res = await fetch(`${API_URL}/api/sessoes`, {
            headers: { 'Authorization': `Bearer ${state.token}` }
        });

        if (!res.ok) throw new Error();

        const sessoes = await res.json();

        if (!sessoes || sessoes.length === 0) {
            grid.innerHTML = `
                        <div class="col-span-full bg-darkCard border border-darkBorder rounded-xl p-8 text-center text-gray-400">
                            Você ainda não possui nenhuma sessão agendada.
                        </div>
                    `;
            return;
        }

        grid.innerHTML = sessoes.map(s => {
            const dataObj = new Date(s.dataHoraUtc);
            const dataFormatada = dataObj.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
            const horaFormatada = dataObj.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });

            return `
                        <div class="bg-darkCard border border-darkBorder rounded-xl p-5 space-y-4 hover:border-gold-500/30 transition-all">
                            <div class="flex items-center justify-between border-b border-darkBorder pb-3">
                                <span class="text-xs font-bold uppercase tracking-wider text-gold-400">
                                    <i class="fa-solid fa-calendar-day mr-1"></i> ${dataFormatada} às ${horaFormatada}
                                </span>
                                <span class="bg-gold-500/10 text-gold-400 border border-gold-500/30 text-[10px] px-2 py-0.5 rounded font-semibold">Confirmado</span>
                            </div>
                            <div class="space-y-1">
                                <p class="text-xs text-gray-400">Tatuador:</p>
                                <p class="text-sm font-bold text-white">${s.nomeTatuador}</p>
                            </div>
                            <div class="space-y-1">
                                <p class="text-xs text-gray-400">Descrição da Arte:</p>
                                <p class="text-xs text-gray-300 italic bg-darkBg p-2.5 rounded border border-darkBorder/60">${s.descricaoTatuagem}</p>
                            </div>
                        </div>
                    `;
        }).join('');

    } catch (err) {
        grid.innerHTML = `
                    <div class="col-span-full bg-darkCard border border-darkBorder rounded-xl p-8 text-center text-red-400">
                        Erro ao carregar seus agendamentos.
                    </div>
                `;
    }
}

// --- HEALTH CHECK DA API ---
async function checkApiHealth() {
    const banner = document.getElementById('apiStatusBanner');
    try {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 3000);

        const res = await fetch(`${API_URL}/health`, { signal: controller.signal });
        clearTimeout(timeoutId);

        if (!res.ok) banner.classList.remove('hidden');
    } catch (err) {
        // Se der timeout ou erro de conexão inicial, mostra o aviso que a Render está acordando
        banner.classList.remove('hidden');
        setTimeout(() => banner.classList.add('hidden'), 12000);
    }
}

// --- MODAIS & NAVEGAÇÃO ---
function openAuthModal(tab = 'login') {
    document.getElementById('modalAuth').classList.remove('hidden');
    switchAuthTab(tab);
}

function closeAuthModal() {
    document.getElementById('modalAuth').classList.add('hidden');
}

function switchAuthTab(tab) {
    const tabL = document.getElementById('tabLogin');
    const tabR = document.getElementById('tabRegistro');
    const formL = document.getElementById('formLogin');
    const formR = document.getElementById('formRegistro');

    if (tab === 'login') {
        tabL.className = "flex-1 pb-3 text-sm font-bold border-b-2 border-gold-500 text-gold-400";
        tabR.className = "flex-1 pb-3 text-sm font-bold border-b-2 border-transparent text-gray-400 hover:text-white";
        formL.classList.remove('hidden');
        formR.classList.add('hidden');
    } else {
        tabR.className = "flex-1 pb-3 text-sm font-bold border-b-2 border-gold-500 text-gold-400";
        tabL.className = "flex-1 pb-3 text-sm font-bold border-b-2 border-transparent text-gray-400 hover:text-white";
        formR.classList.remove('hidden');
        formL.classList.add('hidden');
    }
}

function openTatuadorModal(tatuador) {
    const modal = document.getElementById('modalTatuador');
    const conteudo = document.getElementById('modalTatuadorConteudo');

    conteudo.innerHTML = `
                <div class="text-center space-y-3">
                    <div class="w-24 h-24 rounded-full mx-auto overflow-hidden border-2 border-gold-500/50 shadow-xl">
                        <img src="${mockAvatars[tatuador.id % mockAvatars.length]}" class="w-full h-full object-cover">
                    </div>
                    <div>
                        <h3 class="font-title text-2xl font-bold text-white">${tatuador.nome}</h3>
                        <span class="text-xs text-gold-400 font-semibold uppercase tracking-wider">${tatuador.estiloPrincipal}</span>
                    </div>
                </div>

                <div class="bg-darkBg p-4 rounded-xl border border-darkBorder space-y-2 text-xs text-gray-300 leading-relaxed">
                    <p class="font-bold text-white uppercase text-[10px] tracking-wider text-gold-400">Sobre o Artista:</p>
                    <p>Especialista reconhecido com anos de experiência no mercado de tatuagem autoral. Dedica-se à criação de projetos exclusivos com altíssimo nível de biossegurança e precisão técnica.</p>
                </div>

                <div class="flex items-center justify-between bg-darkBg p-4 rounded-xl border border-darkBorder">
                    <span class="text-xs text-gray-400">Investimento Mínimo:</span>
                    <span class="text-base font-bold text-gold-400">R$ ${tatuador.precoMinimo.toFixed(2)}</span>
                </div>

                <button onclick="closeTatuadorModal(); agendarComTatuador(${tatuador.id})" class="w-full bg-gold-500 hover:bg-gold-400 text-black font-bold text-sm py-3 rounded-lg transition-all shadow-lg flex items-center justify-center gap-2">
                    <i class="fa-solid fa-calendar-plus"></i>
                    <span>Agendar Sessão com este Artista</span>
                </button>
            `;

    modal.classList.remove('hidden');
}

function closeTatuadorModal() {
    document.getElementById('modalTatuador').classList.add('hidden');
}

function agendarComTatuador(id) {
    const select = document.getElementById('selectTatuador');
    if (select) select.value = id;
    window.location.href = '#agendamento';
}

function toggleMobileMenu() {
    const menu = document.getElementById('mobileMenu');
    menu.classList.toggle('hidden');
}

function showNotification(title, message, type = 'info') {
    // Notificação rápida visual simples
    alert(`${title}: ${message}`);
}