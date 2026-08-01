# Configurar o git neste repositório

O que precisa sobreviver não é a Base — ela regenera em três segundos, é
função total da entrada. O que não regenera é `benchmark/results-*`: linha de
base medida, com a solução naquele estado. Hoje ela existe em um lugar só, e
esse lugar já reverteu arquivo uma vez.

---

## 1. Antes de tudo: o `.git` dentro do Google Drive

O repositório está em uma pasta sincronizada. O `.git/` tem milhares de
arquivos pequenos, reescritos a cada operação. Cliente de sync no meio disso
produz corrupção de índice e `.git/index.lock` preso — é a mesma família de
problema que já causou quatro incidentes aqui.

Três saídas, em ordem de preferência:

**A. Mover o repositório para fora do Drive.** `C:\Dev\X7.Knowledge`. O Drive
deixa de ser fator, e some junto a origem das armadilhas já registradas.

**B. Excluir o `.git` da sincronização.** No cliente do Drive:
*Preferências → Pastas do computador → escolher o que sincronizar*. Depende de
o cliente suportar exclusão por subpasta; nem toda versão suporta.

**C. Aceitar o risco.** Funciona a maior parte do tempo. Quando não funcionar,
o sintoma é `fatal: Unable to create '.../.git/index.lock'` ou objeto
corrompido — e aí o remoto é o que salva.

A opção A é a única que resolve. As outras administram.

---

## 2. Iniciar

```bash
cd "D:/Nuvem/GoogleDrive - segundio/X7Dev - Programas/DevEnvironment/Tools/X7.ProjectIndexer"

git init
git config user.name  "seu nome"
git config user.email "seu@email"
```

---

## 3. `.gitignore`

Crie na raiz, antes do primeiro commit:

```gitignore
# Build
bin/
obj/
*.user

# Base publicada — função total da entrada, regenera em segundos
Knowledge/
Base1/
Base2/
*.staging/

# Ferramentas
.vs/
```

`Knowledge/` fica de fora de propósito: versionar saída determinística é ruído
de diff a cada compilação. `benchmark/results-*` **não** entra no ignore — é
exatamente o que precisa ser versionado.

---

## 4. Primeiro commit

```bash
git add .
git status          # confira antes: nada de bin/ e obj/ na lista
git commit -m "X7.Knowledge: C01-C04 com legado v1 removido, esquema 0.7.0"
```

Se `git status` mostrar centenas de arquivos em `bin/`, o `.gitignore` não
pegou. Corrija e rode `git rm -r --cached .` antes de commitar de novo.

---

## 5. Vincular ao remoto

Crie o repositório vazio no GitHub primeiro — **sem** README, sem
`.gitignore`, senão as histórias divergem no primeiro push.

```bash
git remote add origin https://github.com/<usuario>/<repo>.git
git branch -M main
git push -u origin main
```

Autenticação: o GitHub não aceita senha desde 2021. As duas opções são um
*personal access token* usado no lugar da senha, ou o GitHub CLI (`gh auth
login`), que resolve isso sozinho. Não coloque token em arquivo dentro do
repositório.

**Repositório privado.** Não é paranoia: `SECURITY-NOTES.md` registra
nominalmente dez avisos de segurança suprimidos e a análise de por que o risco
é aceitável. É um documento útil para quem mantém e conveniente demais para
quem procura alvo.

---

## 6. Marcar a linha de base

Depois que a medição do C04 estiver gravada:

```bash
git add benchmark/results-c04
git commit -m "Linha de base C04: 5 projetos, snapshot pós-remoção do legado"
git tag c04
git push --tags
```

A tag é o que torna `--until` dispensável no futuro — mas não inútil: ela
recupera o *compilador* daquela época, e `--until` recupera a *Base*. Para
MT-02 a segunda é a certa, porque isola a capacidade da evolução do código.
