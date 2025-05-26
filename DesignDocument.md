# Estimation

**Time Estimation**  
The project will be developed in the following phases:

| Phase                              | Estimated Time  |
|------------------------------------|-----------------|
| Requirement Analysis               | 1 day           |
| System Design                      | 1 day           |
| Development                        | 2 days          |
| &nbsp;&nbsp;&nbsp;&nbsp;- Creating Models                  | 1 hour          |
| &nbsp;&nbsp;&nbsp;&nbsp;- Authentication Module (Keycloak integration) | 2 hours         |
| &nbsp;&nbsp;&nbsp;&nbsp;- Restaurant Module                  | 2 hours         |
| &nbsp;&nbsp;&nbsp;&nbsp;- MenuItem Module                    | 2 hours         |
| &nbsp;&nbsp;&nbsp;&nbsp;- Order & Cart Module               | 3 hours         |
| &nbsp;&nbsp;&nbsp;&nbsp;- DeliveryAgent Module              | 1 hour          |
| &nbsp;&nbsp;&nbsp;&nbsp;- Expose endpoints to a another system    | 2 hour          |
| Testing                            | 1 day           |
| Deployment                         | 1 day           |
| **Total Estimated Time**           | **6 days**      |

---

# Design

## Problem statement
Build a backend API for a food-delivery platform (like Swiggy) that allows customers to browse restaurants and menus, place and track orders, and enables restaurant owners and delivery agents to manage their side of the workflow. Authentication and authorization are handled by Keycloak, while business data lives in PostgreSQL and the API layer is implemented in ASP.NET Core.

## Implementation/Design

### Purpose and Usage
- **Customers** browse restaurants, view menus, add items to cart, place orders, and track delivery status.  
- **Restaurant Owners** manage their restaurant data and menu.  
- **Delivery Agents** claim and update delivery status.  
- **Admins** can oversee all data and troubleshoot.  

### Data Storage Approach
- **PostgreSQL** as the relational store.  
- Use `uuid` (GUID) for primary keys.  
- EF Core for ORM, with navigation properties for one-to-many relationships.
- Tables will reflect relationships: users, restaurants, menus, items, carts, etc.
- Keycloak handles user roles (Customer, RestaurantOwner).

### Database Design Structure Example

**Entities:**
- User (from Keycloak)
- Restaurant (1 → * Menu)
- Menu (1 → * MenuItem)
- Cart (1 per user)
- CartItem (* → 1 MenuItem)

![Swiggy DB design](https://github.com/user-attachments/assets/4113b1ea-ba19-410e-98b1-1a3806ef35e5)


### How Data is Processed
- A customer logs in and fetches a list of available restaurants.
- Upon selecting a restaurant, its Menu and MenuItems are displayed.
- Items are added to the cart. A cart is created if it doesn't exist.
- CartItems are tied to MenuItems and are grouped under the user's Cart.
- Restaurant owners can add/update/delete their menus and menu items.

---

## User Interaction in the API endpoints

1. Login/Register (Keycloak)
2. View list of Restaurants
3. Restaurant Owner: Create/Update Restaurant
4. Restaurant Owner: Add Menu & MenuItems
5. Customer: Browse Restaurant → MenuItems
6. Customer: Add Items to Cart
7. Customer: View/Update/Delete CartItems
8. Customer: Checkout (future)
9. Logout

---

## CRUD Operations

### Restaurant
| Operation | Endpoint                    | Description                  |
|-----------|-----------------------------|------------------------------|
| Create    | POST `/restaurants`         | Add a new restaurant         |
| Read      | GET `/restaurants/{id}`     | Get restaurant details       |
| Update    | PUT `/restaurants/{id}`     | Update restaurant info       |
| Delete    | DELETE `/restaurants/{id}`  | Remove restaurant            |

### Menu & MenuItem
| Operation | Endpoint                              | Description                       |
|-----------|----------------------------------------|-----------------------------------|
| Create    | POST `/restaurants/{id}/menus`         | Add a menu to a restaurant        |
| Create    | POST `/menus/{menuId}/items`           | Add a menu item                   |
| Read      | GET `/menus/{id}`                      | Get menu details                  |
| Read      | GET `/menus/{menuId}/items`            | Get items in a menu               |
| Update    | PUT `/menus/{id}`                      | Update menu                       |
| Update    | PUT `/menuitems/{id}`                  | Update item                       |
| Delete    | DELETE `/menus/{id}`                   | Delete menu                       |
| Delete    | DELETE `/menuitems/{id}`               | Delete menu item                  |
---

### Cart & CartItem
| Operation | Endpoint                                | Description                         |
|-----------|------------------------------------------|-------------------------------------|
| Create    | POST `/users/{id}/cart`                  | Create a cart (if not exists)       |
| Add       | POST `/cart/{id}/items`                  | Add item to cart                    |
| Read      | GET `/cart/{id}`                         | View current cart                   |
| Update    | PUT `/cart/items/{itemId}`               | Update item quantity                |
| Delete    | DELETE `/cart/items/{itemId}`            | Remove item from cart               |
| Delete    | DELETE `/cart/{id}`                      | Empty the cart                      |

## Data Storage Location

- All data is stored in PostgreSQL.
- Authentication and role management are handled by Keycloak (external system).

---

## Design Explanation
- The application uses EF Core to map relational database tables.
- One-to-many and many-to-one relationships are used (e.g., MenuItem → Menu → Restaurant).
- Cart and CartItem allow dynamic, user-specific food selection.
- Role-based access control is delegated to Keycloak.

---

## Non-Functional Requirements (NFRs)
- **Scalability**: Separate entities for restaurants, menu items, and carts make it horizontally scalable.
- **Performance**: Indexed queries and EF Core’s lazy loading will optimize data access.
- **Security**: Keycloak integration ensures secure authentication and role-based authorization.
- **Maintainability**: The modular REST API structure ensures clean separation of responsibilities.
- **Extensibility**: New features like Orders, Reviews, Payments can be easily added.

---

## Alternative Design

### Document-oriented (NoSQL) Approach
- **MongoDB** collections for restaurants, menus, orders (embedded documents).  
- **Pros**: Fewer joins, flexible schema for menu variations.  
- **Cons**: Harder to enforce referential integrity, more complex transactions.  

This relational design offers strong consistency guarantees and is a natural fit for EF Core + PostgreSQL.
