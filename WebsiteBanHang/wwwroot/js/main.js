// Update the base URL below with your running server's address
const API_URL = 'https://localhost:7085/api/products'; // Example for local HTTPS development

async function fetchProducts() {
    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error('Network response was not ok');
        const products = await response.json();
        renderProducts(products);
    } catch (error) {
        console.error('Error fetching products:', error);
        showNotification('Failed to load products. Check console.', 'danger');
    }
}

function renderProducts(products) {
    const list = document.getElementById('product-list');
    list.innerHTML = '';
    products.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';
        card.innerHTML = `
            <img src="${p.imageUrl || 'https://via.placeholder.com/300'}" class="product-img" alt="${p.name}">
            <div class="product-info">
                <h3 class="product-name">${p.name}</h3>
                <div class="product-price">$${p.price.toLocaleString()}</div>
                <p class="product-desc">${p.description || 'No description available.'}</p>
                <div class="actions">
                    <button onclick="editProduct(${p.id})">Edit</button>
                    <button class="btn-danger" onclick="deleteProduct(${p.id})">Delete</button>
                </div>
            </div>
        `;
        list.appendChild(card);
    });
}

document.getElementById('product-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = document.getElementById('product-id').value;
    const product = {
        name: document.getElementById('name').value,
        price: parseFloat(document.getElementById('price').value),
        description: document.getElementById('description').value,
        categoryId: parseInt(document.getElementById('categoryId').value),
        imageUrl: document.getElementById('imageUrl').value || null
    };

    if (id) {
        product.id = parseInt(id);
        await updateProduct(id, product);
    } else {
        await createProduct(product);
    }
    resetForm();
    fetchProducts();
});

async function createProduct(product) {
    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(product)
        });
        if (response.ok) {
            showNotification('Product created successfully!', 'success');
        } else {
            showNotification('Failed to create product.', 'danger');
        }
    } catch (error) {
        console.error('Error creating product:', error);
    }
}

async function updateProduct(id, product) {
    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(product)
        });
        if (response.ok) {
            showNotification('Product updated successfully!', 'success');
        } else {
            showNotification('Failed to update product.', 'danger');
        }
    } catch (error) {
        console.error('Error updating product:', error);
    }
}

async function deleteProduct(id) {
    if (!confirm('Are you sure you want to delete this product?')) return;
    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: 'DELETE'
        });
        if (response.ok) {
            showNotification('Product deleted successfully!', 'success');
            fetchProducts();
        } else {
            showNotification('Failed to delete product.', 'danger');
        }
    } catch (error) {
        console.error('Error deleting product:', error);
    }
}

async function editProduct(id) {
    try {
        const response = await fetch(`${API_URL}/${id}`);
        const p = await response.json();
        document.getElementById('product-id').value = p.id;
        document.getElementById('name').value = p.name;
        document.getElementById('price').value = p.price;
        document.getElementById('description').value = p.description || '';
        document.getElementById('categoryId').value = p.categoryId;
        document.getElementById('imageUrl').value = p.imageUrl || '';
        document.getElementById('form-title').innerText = 'Edit Product';
        document.getElementById('submit-btn').innerText = 'Update Product';
        document.getElementById('cancel-btn').style.display = 'block';
        window.scrollTo({ top: 0, behavior: 'smooth' });
    } catch (error) {
        console.error('Error fetching product for edit:', error);
    }
}

function resetForm() {
    document.getElementById('product-id').value = '';
    document.getElementById('product-form').reset();
    document.getElementById('form-title').innerText = 'Add New Product';
    document.getElementById('submit-btn').innerText = 'Add Product';
    document.getElementById('cancel-btn').style.display = 'none';
}

function showNotification(message, type) {
    const notify = document.getElementById('notification');
    notify.innerText = message;
    notify.style.backgroundColor = type === 'success' ? '#22c55e' : '#ef4444';
    notify.style.display = 'block';
    setTimeout(() => { notify.style.display = 'none'; }, 3000);
}

// Initial load
fetchProducts();
