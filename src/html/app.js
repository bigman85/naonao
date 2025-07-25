// API配置
const API_BASE_URL = 'http://localhost:5000/api';
let authToken = localStorage.getItem('authToken');

// 初始化应用
document.addEventListener('DOMContentLoaded', function() {
    if (authToken) {
        showMainApp();
        loadDashboard();
    } else {
        showLoginPage();
    }
    
    setupEventListeners();
});

// 事件监听器设置
function setupEventListeners() {
    document.getElementById('loginForm').addEventListener('submit', handleLogin);
    document.getElementById('logoutBtn').addEventListener('click', handleLogout);
    document.getElementById('refreshBtn').addEventListener('click', refreshCurrentPage);
    
    // 导航菜单
    document.querySelectorAll('[data-page]').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const page = this.getAttribute('data-page');
            switchPage(page);
            
            // 更新活动状态
            document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

// 认证相关
async function handleLogin(e) {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, password })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            authToken = data.accessToken;
            localStorage.setItem('authToken', authToken);
            localStorage.setItem('refreshToken', data.refreshToken);
            showMainApp();
            loadDashboard();
        } else {
            alert(data.message || '登录失败');
        }
    } catch (error) {
        console.error('登录错误:', error);
        alert('登录失败，请检查网络连接');
    }
}

function handleLogout() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    authToken = null;
    showLoginPage();
}

// 页面切换
function showLoginPage() {
    document.getElementById('loginPage').classList.remove('hidden');
    document.getElementById('mainApp').classList.add('hidden');
}

function showMainApp() {
    document.getElementById('loginPage').classList.add('hidden');
    document.getElementById('mainApp').classList.remove('hidden');
}

function switchPage(page) {
    // 隐藏所有页面
    document.querySelectorAll('.page-content').forEach(content => {
        content.classList.add('hidden');
    });
    
    // 显示目标页面
    document.getElementById(`${page}Page`).classList.remove('hidden');
    
    // 更新页面标题
    const titles = {
        dashboard: '仪表盘',
        users: '用户管理',
        roles: '角色管理',
        permissions: '权限管理'
    };
    document.getElementById('pageTitle').textContent = titles[page];
    
    // 加载对应页面数据
    switch(page) {
        case 'dashboard':
            loadDashboard();
            break;
        case 'users':
            loadUsers();
            break;
        case 'roles':
            loadRoles();
            break;
        case 'permissions':
            loadPermissions();
            break;
    }
}

// API请求工具函数
async function apiRequest(endpoint, options = {}) {
    const config = {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${authToken}`,
            ...options.headers
        }
    };
    
    const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
    
    if (response.status === 401) {
        // 尝试刷新token
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
            const refreshResponse = await fetch(`${API_BASE_URL}/auth/refresh`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken })
            });
            
            if (refreshResponse.ok) {
                const refreshData = await refreshResponse.json();
                authToken = refreshData.accessToken;
                localStorage.setItem('authToken', authToken);
                localStorage.setItem('refreshToken', refreshData.refreshToken);
                
                // 重试原请求
                config.headers.Authorization = `Bearer ${authToken}`;
                return fetch(`${API_BASE_URL}${endpoint}`, config);
            }
        }
        
        handleLogout();
        throw new Error('未授权');
    }
    
    return response;
}

// 仪表盘
async function loadDashboard() {
    try {
        const [usersResponse, rolesResponse, permissionsResponse] = await Promise.all([
            apiRequest('/users'),
            apiRequest('/roles'),
            apiRequest('/permissions')
        ]);
        
        if (usersResponse.ok && rolesResponse.ok && permissionsResponse.ok) {
            const users = await usersResponse.json();
            const roles = await rolesResponse.json();
            const permissions = await permissionsResponse.json();
            
            document.getElementById('totalUsers').textContent = users.length || 0;
            document.getElementById('totalRoles').textContent = roles.length || 0;
            document.getElementById('totalPermissions').textContent = permissions.length || 0;
        }
    } catch (error) {
        console.error('加载仪表盘数据失败:', error);
    }
}

// 用户管理
async function loadUsers() {
    try {
        const response = await apiRequest('/users');
        if (response.ok) {
            const users = await response.json();
            displayUsers(users);
        }
    } catch (error) {
        console.error('加载用户失败:', error);
    }
}

function displayUsers(users) {
    const tbody = document.getElementById('usersTableBody');
    tbody.innerHTML = '';
    
    users.forEach(user => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${user.username}</td>
            <td>${user.email}</td>
            <td>${user.roles.join(', ')}</td>
            <td>${new Date(user.createdAt).toLocaleDateString()}</td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="editUser('${user.id}')">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteUser('${user.id}')">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

async function saveUser() {
    const formData = {
        username: document.getElementById('userUsername').value,
        email: document.getElementById('userEmail').value,
        password: document.getElementById('userPassword').value,
        fullName: document.getElementById('userFullName').value,
        roles: Array.from(document.getElementById('userRoles').selectedOptions).map(option => option.value)
    };
    
    try {
        const response = await apiRequest('/users', {
            method: 'POST',
            body: JSON.stringify(formData)
        });
        
        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('userModal')).hide();
            document.getElementById('userForm').reset();
            loadUsers();
        } else {
            const error = await response.json();
            alert(error.message || '保存用户失败');
        }
    } catch (error) {
        console.error('保存用户失败:', error);
        alert('保存用户失败');
    }
}

// 角色管理
async function loadRoles() {
    try {
        const response = await apiRequest('/roles');
        if (response.ok) {
            const roles = await response.json();
            displayRoles(roles);
            
            // 更新用户角色选择器
            const roleSelect = document.getElementById('userRoles');
            roleSelect.innerHTML = '';
            roles.forEach(role => {
                const option = document.createElement('option');
                option.value = role.name;
                option.textContent = role.name;
                roleSelect.appendChild(option);
            });
        }
    } catch (error) {
        console.error('加载角色失败:', error);
    }
}

function displayRoles(roles) {
    const tbody = document.getElementById('rolesTableBody');
    tbody.innerHTML = '';
    
    roles.forEach(role => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${role.name}</td>
            <td>${role.description || ''}</td>
            <td>${role.userCount || 0}</td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="editRole('${role.id}')">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteRole('${role.id}')">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

async function saveRole() {
    const formData = {
        name: document.getElementById('roleName').value,
        description: document.getElementById('roleDescription').value
    };
    
    try {
        const response = await apiRequest('/roles', {
            method: 'POST',
            body: JSON.stringify(formData)
        });
        
        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('roleModal')).hide();
            document.getElementById('roleForm').reset();
            loadRoles();
        } else {
            const error = await response.json();
            alert(error.message || '保存角色失败');
        }
    } catch (error) {
        console.error('保存角色失败:', error);
        alert('保存角色失败');
    }
}

// 权限管理
async function loadPermissions() {
    try {
        const response = await apiRequest('/permissions');
        if (response.ok) {
            const permissions = await response.json();
            displayPermissions(permissions);
        }
    } catch (error) {
        console.error('加载权限失败:', error);
    }
}

function displayPermissions(permissions) {
    const tbody = document.getElementById('permissionsTableBody');
    tbody.innerHTML = '';
    
    function renderPermissions(permissions, level = 0) {
        permissions.forEach(permission => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td style="padding-left: ${level * 20}px">${permission.name}</td>
                <td>${permission.code}</td>
                <td>${permission.resourceType}</td>
                <td>
                    <button class="btn btn-sm btn-primary" onclick="editPermission('${permission.id}')">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deletePermission('${permission.id}')">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(row);
            
            if (permission.children && permission.children.length > 0) {
                renderPermissions(permission.children, level + 1);
            }
        });
    }
    
    renderPermissions(permissions);
}

async function savePermission() {
    const formData = {
        name: document.getElementById('permissionName').value,
        code: document.getElementById('permissionCode').value,
        resourceType: document.getElementById('permissionResourceType').value,
        resourcePath: document.getElementById('permissionResourcePath').value
    };
    
    try {
        const response = await apiRequest('/permissions', {
            method: 'POST',
            body: JSON.stringify(formData)
        });
        
        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('permissionModal')).hide();
            document.getElementById('permissionForm').reset();
            loadPermissions();
        } else {
            const error = await response.json();
            alert(error.message || '保存权限失败');
        }
    } catch (error) {
        console.error('保存权限失败:', error);
        alert('保存权限失败');
    }
}

// 工具函数
function refreshCurrentPage() {
    const activePage = document.querySelector('.nav-link.active').getAttribute('data-page');
    switchPage(activePage);
}

// 编辑和删除功能（简化版本）
function editUser(id) {
    alert(`编辑用户: ${id} (功能待实现)`);
}

function deleteUser(id) {
    if (confirm('确定要删除这个用户吗？')) {
        alert(`删除用户: ${id} (功能待实现)`);
    }
}

function editRole(id) {
    alert(`编辑角色: ${id} (功能待实现)`);
}

function deleteRole(id) {
    if (confirm('确定要删除这个角色吗？')) {
        alert(`删除角色: ${id} (功能待实现)`);
    }
}

function editPermission(id) {
    alert(`编辑权限: ${id} (功能待实现)`);
}

function deletePermission(id) {
    if (confirm('确定要删除这个权限吗？')) {
        alert(`删除权限: ${id} (功能待实现)`);
    }
}