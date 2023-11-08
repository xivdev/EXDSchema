for d in * ; do
    echo "Zipping $d"
    zip -rq -9 ../release-out/$d.zip $d
done