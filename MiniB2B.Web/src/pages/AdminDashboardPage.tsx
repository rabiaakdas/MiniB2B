import { Link } from 'react-router-dom';

const dashboardSections = [
  {
    title: 'Ürün Yönetimi',
    description: 'Ürünleri görüntüleyin, ekleyin ve düzenleyin.',
    action: 'Ürünlere Git',
    to: '/admin/products',
  },
  {
    title: 'Sipariş Yönetimi',
    description: 'Siparişleri görüntüleyin ve durumlarını yönetin.',
    action: 'Siparişlere Git',
    to: '/admin/orders',
  },
  {
    title: 'Kullanıcı Yönetimi',
    description: 'Kayıtlı kullanıcıları görüntüleyin ve düzenleyin.',
    action: 'Kullanıcılara Git',
    to: '/admin/users',
  },
  {
    title: 'Banner Yönetimi',
    description: 'Ana sayfa banner içeriklerini yönetin.',
    action: 'Bannerlara Git',
    to: '/admin/banners',
  },
];

export function AdminDashboardPage() {
  return (
    <section className="page-stack">
      <div className="page-panel admin-dashboard-hero">
        <p className="eyebrow">Admin</p>
        <h1>Yönetim Paneli</h1>
        <p className="muted">Ürün, sipariş, kullanıcı ve banner işlemlerini buradan yönetebilirsiniz.</p>
      </div>

      <div className="admin-dashboard-grid">
        {dashboardSections.map((section) => (
          <article className="admin-dashboard-card" key={section.to}>
            <div>
              <h2>{section.title}</h2>
              <p>{section.description}</p>
            </div>
            <Link className="text-link" to={section.to}>
              {section.action}
            </Link>
          </article>
        ))}
      </div>
    </section>
  );
}
