import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { Trash } from "lucide-react";
import { getAllCarsOptions } from "../api/getAllCarsOptions";
import { useDeleteCar } from "../api/useDeleteCar";
import EditCarDialog from "./EditCarDialog";
import ViewCarDialog from "./ViewCarDialog";

type CarListTableProps = {
  className?: string;
};

const CarListTable = ({ className }: CarListTableProps) => {
  const { data: carsQuery } = useQuery(getAllCarsOptions);
  const cars = carsQuery?.data || [];

  const { mutateAsync: deleteCarAsync } = useDeleteCar();

  const handleDelete = async (id: string) => {
    await deleteCarAsync({ id });
  };

  return (
    <div className={cn(className)}>
      <Table>
        <TableCaption>A list of Cars</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead>Year</TableHead>
            <TableHead>Make</TableHead>
            <TableHead>Model</TableHead>
            <TableHead>Fuel type</TableHead>
            <TableHead>Transmission</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {cars.map((c) => (
            <TableRow key={c.id}>
              <TableCell>{c.year}</TableCell>
              <TableCell>{c.make}</TableCell>
              <TableCell>{c.model}</TableCell>
              <TableCell>{c.fuelType}</TableCell>
              <TableCell>{c.transmission}</TableCell>
              <TableCell>
                <ViewCarDialog id={c.id} />
                <EditCarDialog id={c.id} />
                <Button variant="secondary" onClick={() => handleDelete(c.id)}>
                  <Trash />
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
};

export default CarListTable;
