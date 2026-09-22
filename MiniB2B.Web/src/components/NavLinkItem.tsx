import { NavLink } from 'react-router-dom';

interface NavLinkItemProps {
  to: string;
  children: string;
  end?: boolean;
}

export function NavLinkItem({ to, children, end }: NavLinkItemProps) {
  return (
    <NavLink className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')} to={to} end={end}>
      {children}
    </NavLink>
  );
}
