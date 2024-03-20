//
const GAMES = [//
    'checkboxes.js',//
    'flipper.js',//
    'life.js',//
];

let CURRENT_GAME = undefined;

export default async () => {
    let game_name = CURRENT_GAME;
    while (game_name === CURRENT_GAME)
            game_name = getRandom(GAMES);
    CURRENT_GAME = game_name;

    const game = await import('https://sidneys1.github.io/assets/404/' + CURRENT_GAME);

    document.getElementById('load-game-button').innerText = '🎲 Load another random game...';
    document.getElementById('game-root').innerHTML = `<h2>${game.NAME}</h2><p>${game.DESCRIPTION}</p><div id="game-board"></div>`;

    console.log(`Loading ${game.NAME}!`);
    game.default();
}

/////////////////////////////////// Utils

export const shuffle = array => {
    for (var i = array.length - 1; i > 0; i--) {
        var j = Math.floor(Math.random() * (i + 1));
        var temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }
}

export const getRandom = array => array[Math.floor(Math.random() * array.length)];
