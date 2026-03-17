// Player State
let currentSong = null;
let isPlaying = false;
let playlist = [];
let currentIndex = -1;
let isShuffle = false;
let repeatMode = 0; // 0 = off, 1 = repeat all, 2 = repeat one

// DOM Elements
const audioPlayer = document.getElementById('audioPlayer');
const playerBar = document.getElementById('playerBar');
const btnPlay = document.getElementById('btnPlay');
const btnPrevious = document.getElementById('btnPrevious');
const btnNext = document.getElementById('btnNext');
const btnShuffle = document.getElementById('btnShuffle');
const btnRepeat = document.getElementById('btnRepeat');
const btnLike = document.getElementById('btnLike');
const btnVolume = document.getElementById('btnVolume');
const volumeSlider = document.getElementById('volumeSlider');
const progressBar = document.getElementById('progressBar');
const progress = document.getElementById('progress');
const currentTime = document.getElementById('currentTime');
const duration = document.getElementById('duration');
const currentSongTitle = document.getElementById('currentSongTitle');
const currentSongArtist = document.getElementById('currentSongArtist');

// Play Song
function playSong(element) {
    const songId = element.dataset.songId;
    const title = element.dataset.songTitle;
    const artists = element.dataset.songArtists;
    const url = element.dataset.songUrl;
    const cover = element.dataset.songCover || '';

    currentSong = { id: songId, title, artists, url, cover };

    // Update UI text
    currentSongTitle.textContent = title;
    currentSongArtist.textContent = artists;

    // Update album art thumbnail
    const thumb = document.getElementById('playerThumb');
    if (thumb) {
        if (cover) {
            thumb.innerHTML = `<img src="${cover}" alt="${title}" />`;
        } else {
            thumb.innerHTML = '<i class="fas fa-music"></i>';
        }
    }

    // Show player bar
    playerBar.classList.add('active');

    // Load and play
    audioPlayer.src = url;
    audioPlayer.play();
    isPlaying = true;
    playerBar.classList.add('playing');
    updatePlayButton();

    // Build playlist from all song cards
    const allCards = document.querySelectorAll('.song-card');
    playlist = Array.from(allCards).map(c => ({
        id: c.dataset.songId,
        title: c.dataset.songTitle,
        artists: c.dataset.songArtists,
        url: c.dataset.songUrl,
        cover: c.dataset.songCover || ''
    }));
    currentIndex = playlist.findIndex(s => s.id === songId);

    // Highlight active card
    allCards.forEach(c => c.classList.remove('now-playing'));
    const activeCard = document.querySelector(`.song-card[data-song-id="${songId}"]`);
    if (activeCard) activeCard.classList.add('now-playing');

    // Track play count
    trackPlay(songId);
}

// Helper: play song by index
function playSongByIndex(index) {
    if (index < 0 || index >= playlist.length) return;
    currentIndex = index;
    const song = playlist[index];
    const fakeEl = { dataset: { songId: song.id, songTitle: song.title, songArtists: song.artists, songUrl: song.url, songCover: song.cover } };
    playSong(fakeEl);
}

// Play/Pause Toggle
btnPlay.addEventListener('click', () => {
    if (isPlaying) {
        audioPlayer.pause();
        isPlaying = false;
        playerBar.classList.remove('playing');
    } else {
        audioPlayer.play();
        isPlaying = true;
        playerBar.classList.add('playing');
    }
    updatePlayButton();
});

// ===== PREVIOUS =====
btnPrevious.addEventListener('click', () => {
    if (playlist.length === 0) return;

    // If more than 3 seconds played, restart the song
    if (audioPlayer.currentTime > 3) {
        audioPlayer.currentTime = 0;
        return;
    }

    if (isShuffle) {
        playSongByIndex(Math.floor(Math.random() * playlist.length));
    } else if (currentIndex > 0) {
        playSongByIndex(currentIndex - 1);
    } else if (repeatMode === 1) {
        // Repeat all: go to last song
        playSongByIndex(playlist.length - 1);
    }
});

// ===== NEXT =====
btnNext.addEventListener('click', () => {
    if (playlist.length === 0) return;

    if (isShuffle) {
        playSongByIndex(Math.floor(Math.random() * playlist.length));
    } else if (currentIndex < playlist.length - 1) {
        playSongByIndex(currentIndex + 1);
    } else if (repeatMode === 1) {
        // Repeat all: go back to first song
        playSongByIndex(0);
    }
});

// ===== SHUFFLE =====
btnShuffle.addEventListener('click', () => {
    isShuffle = !isShuffle;
    btnShuffle.classList.toggle('active', isShuffle);
    if (isShuffle) {
        btnShuffle.style.color = 'var(--green, #1db954)';
    } else {
        btnShuffle.style.color = '';
    }
});

// ===== REPEAT =====
btnRepeat.addEventListener('click', () => {
    repeatMode = (repeatMode + 1) % 3;
    const icon = btnRepeat.querySelector('i');

    if (repeatMode === 0) {
        // Off
        btnRepeat.style.color = '';
        icon.className = 'fas fa-repeat';
        btnRepeat.classList.remove('active');
    } else if (repeatMode === 1) {
        // Repeat all
        btnRepeat.style.color = 'var(--green, #1db954)';
        icon.className = 'fas fa-repeat';
        btnRepeat.classList.add('active');
    } else {
        // Repeat one
        btnRepeat.style.color = 'var(--green, #1db954)';
        icon.className = 'fas fa-repeat';
        btnRepeat.setAttribute('data-repeat-one', '1');
        btnRepeat.classList.add('active');
    }
});

// Update Play Button Icon
function updatePlayButton() {
    const icon = btnPlay.querySelector('i');
    if (isPlaying) {
        icon.className = 'fas fa-pause';
    } else {
        icon.className = 'fas fa-play';
    }
}

// Audio Events
audioPlayer.addEventListener('timeupdate', () => {
    if (audioPlayer.duration) {
        const percent = (audioPlayer.currentTime / audioPlayer.duration) * 100;
        progress.style.width = percent + '%';
        currentTime.textContent = formatTime(audioPlayer.currentTime);
        duration.textContent = formatTime(audioPlayer.duration);
    }
});

// ===== AUTO-PLAY NEXT (with repeat/shuffle support) =====
audioPlayer.addEventListener('ended', () => {
    isPlaying = false;
    playerBar.classList.remove('playing');
    updatePlayButton();

    if (repeatMode === 2) {
        // Repeat one: replay the same song
        audioPlayer.currentTime = 0;
        audioPlayer.play();
        isPlaying = true;
        playerBar.classList.add('playing');
        updatePlayButton();
    } else if (isShuffle) {
        // Shuffle: pick a random song
        playSongByIndex(Math.floor(Math.random() * playlist.length));
    } else if (currentIndex < playlist.length - 1) {
        // Next song
        playSongByIndex(currentIndex + 1);
    } else if (repeatMode === 1) {
        // Repeat all: go back to first
        playSongByIndex(0);
    }
});

// Progress Bar Click
progressBar.addEventListener('click', (e) => {
    if (audioPlayer.duration) {
        const rect = progressBar.getBoundingClientRect();
        const percent = (e.clientX - rect.left) / rect.width;
        audioPlayer.currentTime = percent * audioPlayer.duration;
    }
});

// Volume Control
volumeSlider.addEventListener('input', (e) => {
    audioPlayer.volume = e.target.value / 100;
    updateVolumeIcon();
});

btnVolume.addEventListener('click', () => {
    if (audioPlayer.volume > 0) {
        audioPlayer.volume = 0;
        volumeSlider.value = 0;
    } else {
        audioPlayer.volume = 0.8;
        volumeSlider.value = 80;
    }
    updateVolumeIcon();
});

function updateVolumeIcon() {
    const icon = btnVolume.querySelector('i');
    if (audioPlayer.volume === 0) {
        icon.className = 'fas fa-volume-mute';
    } else if (audioPlayer.volume < 0.5) {
        icon.className = 'fas fa-volume-down';
    } else {
        icon.className = 'fas fa-volume-up';
    }
}

// Format Time (seconds to mm:ss)
function formatTime(seconds) {
    if (!seconds || isNaN(seconds)) return '0:00';
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
}

// Like Button
btnLike.addEventListener('click', async () => {
    if (!currentSong) return;

    try {
        const response = await fetch(`/Song/Like/${currentSong.id}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (data.success) {
            const icon = btnLike.querySelector('i');
            if (data.liked) {
                icon.className = 'fas fa-heart text-danger';
            } else {
                icon.className = 'far fa-heart';
            }
        }
    } catch (error) {
        console.error('Error liking song:', error);
    }
});

// Track Play Count
async function trackPlay(songId) {
    try {
        await fetch(`/Song/TrackPlay/${songId}`, {
            method: 'POST'
        });
    } catch (error) {
        console.error('Error tracking play:', error);
    }
}

// Search
const searchInput = document.getElementById('searchInput');
if (searchInput) {
    let searchTimeout;
    searchInput.addEventListener('input', (e) => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            const query = e.target.value.trim();
            if (query.length > 0) {
                window.location.href = `/Home/Search?q=${encodeURIComponent(query)}`;
            }
        }, 500);
    });
}

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    // Set initial volume
    audioPlayer.volume = 0.8;
});



document.addEventListener('contextmenu', async (e) => {
    const songCard = e.target.closest('.song-card');
    if (songCard) {
        e.preventDefault();

        const songId = songCard.dataset.songId;
        const title = songCard.dataset.songTitle;

        await showAddToPlaylistMenu(songId, title);
    }
});

async function showAddToPlaylistMenu(songId, songTitle) {
    // Fetch user playlists
    try {
        const response = await fetch('/Playlist/GetUserPlaylists');
        const data = await response.json();

        if (data.success && data.playlists.length > 0) {
            // Show modal with playlists
            const modal = document.createElement('div');
            modal.className = 'modal fade';
            modal.innerHTML = `
                <div class="modal-dialog">
                    <div class="modal-content bg-dark text-white">
                        <div class="modal-header border-secondary">
                            <h5 class="modal-title">Thêm vào playlist</h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p class="text-muted mb-3">Bài hát: ${songTitle}</p>
                            <div class="list-group">
                                ${data.playlists.map(p => `
                                    <button class="list-group-item list-group-item-action bg-dark text-white border-secondary"
                                            onclick="addToPlaylist('${p.playlistId}', '${songId}')">
                                        <i class="fas fa-list me-2"></i> ${p.playlistName}
                                    </button>
                                `).join('')}
                            </div>
                        </div>
                    </div>
                </div>
            `;

            document.body.appendChild(modal);
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();

            modal.addEventListener('hidden.bs.modal', () => {
                modal.remove();
            });
        } else {
            alert('Bạn chưa có playlist nào. Tạo playlist trước!');
        }
    } catch (error) {
        console.error('Error:', error);
    }
}

async function addToPlaylist(playlistId, songId) {
    try {
        const response = await fetch('/Playlist/AddSong', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: new URLSearchParams({
                playlistId: playlistId,
                songId: songId
            })
        });

        const data = await response.json();

        if (data.success) {
            alert('Đã thêm vào playlist!');
            // Close modal
            const modal = document.querySelector('.modal.show');
            if (modal) {
                bootstrap.Modal.getInstance(modal).hide();
            }
        } else {
            alert('Lỗi: ' + data.message);
        }
    } catch (error) {
        alert('Lỗi: ' + error.message);
    }
}