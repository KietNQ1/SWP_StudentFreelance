/**
 * Search functionality for the header search bar
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeSearch();
});

function initializeSearch() {
    const searchButton = document.getElementById('searchButton');
    const searchInput = document.getElementById('searchInput');
    const searchType = document.getElementById('searchType');
    
    if (!searchButton || !searchInput || !searchType) return;
    
    // Set search type from URL if available
    const urlParams = new URLSearchParams(window.location.search);
    const currentPage = window.location.pathname.toLowerCase();
    
    // Set default search type based on current page
    if (currentPage.includes('/search/searchjob')) {
        searchType.value = 'project';
        searchInput.value = urlParams.get('query') || '';
    } else if (currentPage.includes('/search/searchstudents')) {
        searchType.value = 'student';
        searchInput.value = urlParams.get('query') || '';
    } else if (currentPage.includes('/search/searchbusinesses')) {
        searchType.value = 'business';
        searchInput.value = urlParams.get('query') || '';
    }
    
    // Add event listeners
    searchButton.addEventListener('click', function() {
        performSearch();
    });
    
    searchInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            performSearch();
        }
    });
    
    // Auto-focus search input when dropdown changes
    searchType.addEventListener('change', function() {
        searchInput.focus();
    });
}

function performSearch() {
    const searchInput = document.getElementById('searchInput');
    const searchType = document.getElementById('searchType');
    
    const query = searchInput.value.trim();
    const type = searchType.value;
    
    if (query) {
        let url = '';
        switch(type) {
            case 'project':
                url = '/Search/SearchJob?query=' + encodeURIComponent(query);
                break;
            case 'student':
                url = '/Search/SearchStudents?query=' + encodeURIComponent(query);
                break;
            case 'business':
                url = '/Search/SearchBusinesses?query=' + encodeURIComponent(query);
                break;
        }
        
        window.location.href = url;
    }
}

// Function to search with additional parameters
function advancedSearch(additionalParams) {
    const searchInput = document.getElementById('searchInput');
    const searchType = document.getElementById('searchType');
    
    const query = searchInput.value.trim();
    const type = searchType.value;
    
    let url = '';
    switch(type) {
        case 'project':
            url = '/Search/SearchJob?query=' + encodeURIComponent(query);
            break;
        case 'student':
            url = '/Search/SearchStudents?query=' + encodeURIComponent(query);
            break;
        case 'business':
            url = '/Search/SearchBusinesses?query=' + encodeURIComponent(query);
            break;
    }
    
    // Add additional parameters
    if (additionalParams) {
        for (const [key, value] of Object.entries(additionalParams)) {
            if (value) {
                url += `&${key}=${encodeURIComponent(value)}`;
            }
        }
    }
    
    window.location.href = url;
} 